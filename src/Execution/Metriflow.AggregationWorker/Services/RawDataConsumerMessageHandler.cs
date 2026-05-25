using System.Threading.Channels;
using Metriflow.AggregationWorker.Interfaces.Correlation;
using Metriflow.Application.Entities;
using Metriflow.Application.Interfaces;
using Metriflow.Domain.Entities.Workers;
using Metriflow.Domain.enums;
using Metriflow.Domain.Interfaces;
using Metriflow.Messages.Producers;
using Microsoft.Extensions.Options;

namespace Metriflow.AggregationWorker.Services;

/// <summary>
/// Default implementation of <see cref="IRawDataConsumerMessageHandler{T}"/>. Stores incoming records in Redis,
/// tracks the maximum received date, and triggers matching/combining logic when appropriate.
/// </summary>
public class RawDataConsumerMessageHandler<T>(
    ILogger<RawDataConsumerMessageHandler<T>> logger,
    IOptions<RabbitMqSettings> options,
    INotifyWorkers notifyWorkers,
    IServiceScopeFactory scopeFactory
) : IRawDataConsumerMessageHandler<T>
    where T : class, IAnalyticRecord
{
    private readonly RabbitMqSettings _rabbitMqSettings = options.Value;
    private const int BatchCount = (int)enBatchSizes.RawDataBaseBatch;
    private readonly TimeSpan _flushTimeout = TimeSpan.FromSeconds(10);
    private readonly List<List<T>> _outerRecordsLst = new();
    private int _accumulator;

    public async Task HandleIncomingAnalyticsRecordsAsync(
        Channel<List<T>> channel,
        CancellationToken stoppingToken
    )
    {
        try
        {
            await this.Process(channel, stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
    }

    private async Task Process(Channel<List<T>> channel, CancellationToken stoppingToken)
    {
        logger.LogInformation($"Processing {typeof(T).Name}");
        try
        {
            while (true)
            {
                try
                {
                    // Create a token that cancels after flushTimeout, linked to the main stoppingToken
                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(
                        stoppingToken
                    );

                    timeoutCts.CancelAfter(_flushTimeout);

                    bool dataAvailable = await channel.Reader.WaitToReadAsync(timeoutCts.Token);

                    // Channel is completed
                    if (!dataAvailable)
                        break;

                    while (channel.Reader.TryRead(out var lst))
                    {
                        _outerRecordsLst.Add(lst);
                        _accumulator += lst.Count;

                        if (_accumulator >= BatchCount)
                        {
                            await Flush(stoppingToken);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    if (_outerRecordsLst.Count > 0)
                    {
                        await Flush(stoppingToken);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
        finally
        {
            //should also read remains items in the channel but I'll skip it for now

            if (_outerRecordsLst.Count > 0)
            {
                await Flush(stoppingToken);
            }
        }
    }

    async Task Flush(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "@@@@@@ Flushing {Count} {RecordType} records to database...",
            _accumulator,
            typeof(T).Name
        );

        using var scope = scopeFactory.CreateScope();

        var repo = scope.ServiceProvider.GetRequiredService<IRecordBatchSaver<T>>();

        await repo.SaveBulkAsync(_outerRecordsLst, _accumulator);

        var stagingQueue = typeof(T) == typeof(GARecord)
            ? _rabbitMqSettings.Queues.StagingGA
            : _rabbitMqSettings.Queues.StagingPSA;

        await notifyWorkers.Notify(
            _accumulator,
            AggregationType.Records,
            stagingQueue,
            stoppingToken
        );

        _outerRecordsLst.Clear();
        _accumulator = 0;
    }
}