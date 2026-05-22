using System.Threading.Channels;
using Metriflow.AggregationWorker.Interfaces.Correlation;
using Metriflow.Application.Entities;
using Metriflow.Application.Interfaces;
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
    const int batchCount = (int)enBatchSizes.RawDataBaseBatch;
    static readonly TimeSpan flushTimeout = TimeSpan.FromSeconds(50);
    List<List<T>> outerRecordsLst = new();
    int accumulator = 0;

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
                    timeoutCts.CancelAfter(flushTimeout);

                    // This will throw OperationCanceledException if flushTimeout is reached BEFORE a message arrives
                    bool dataAvailable = await channel.Reader.WaitToReadAsync(timeoutCts.Token);

                    if (!dataAvailable)
                        break; // Channel is completed

                    while (channel.Reader.TryRead(out var lst))
                    {
                        outerRecordsLst.Add(lst);
                        accumulator += lst.Count;

                        if (accumulator >= batchCount)
                        {
                            await Flush(stoppingToken);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // If stoppingToken was canceled, exit the loop.
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    // Otherwise, it was just our timeoutCts. Flush if we have data!
                    if (outerRecordsLst.Count > 0)
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

            if (outerRecordsLst.Count > 0)
            {
                await Flush(stoppingToken);
            }
        }
    }

    async Task Flush(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "@@@@@@ Flushing {Count} {RecordType} records to database...",
            accumulator,
            typeof(T).Name
        );

        using var scope = scopeFactory.CreateScope();

        var repo = scope.ServiceProvider.GetRequiredService<IRecordBatchSaver<T>>();

        await repo.SaveBulkAsync(outerRecordsLst, accumulator);

        await notifyWorkers.Notify(
            accumulator,
            AggregationType.Records,
            _rabbitMqSettings.Queues.StagingData,
            stoppingToken
        );

        outerRecordsLst.Clear();
        accumulator = 0;
    }
}