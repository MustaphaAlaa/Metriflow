using System.Threading.Channels;
using Metriflow.AggregationWorker.Interfaces.Correlation;
using Metriflow.Application.Interfaces;
using Metriflow.Domain.Interfaces;

namespace Metriflow.AggregationWorker.Services;

/// <summary>
/// Default implementation of <see cref="IRawDataConsumerMessageHandler{T}"/>. Stores incoming records in Redis,
/// tracks the maximum received date, and triggers matching/combining logic when appropriate.
/// </summary>
public class RawDataConsumerMessageHandler<T>(
    ILogger<RawDataConsumerMessageHandler<T>> logger,
    IServiceScopeFactory scopeFactory
) : IRawDataConsumerMessageHandler<T>
    where T : class, IAnalyticRecord
{
    const int batchCount = 150_000;
    static readonly TimeSpan flushTimeout = TimeSpan.FromSeconds(50);

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
        var accumulator = 0;
        List<List<T>> outerRecordsLst = new List<List<T>>();

        logger.LogInformation($"Processing {nameof(T)}");
        // );
        try
        {
            var date = DateTime.UtcNow;
            var lastReceiveTime = DateTime.UtcNow;
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
                            await Flush(outerRecordsLst, accumulator);
                            outerRecordsLst.Clear();
                            accumulator = 0;
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
                        await Flush(outerRecordsLst, accumulator);
                        outerRecordsLst.Clear();
                        accumulator = 0;
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
                await Flush(outerRecordsLst, accumulator);
                outerRecordsLst.Clear();
                accumulator = 0;
            }
        }
    }

    async Task Flush(List<List<T>> outerRecordsLst, int accumulator)
    {
        logger.LogInformation(
            "@@@@@@ Flushing {Count} {RecordType} records to database...",
            accumulator,
            typeof(T).Name
        );

        using var scope = scopeFactory.CreateScope();

        var repo = scope.ServiceProvider.GetRequiredService<IRecordBatchSaver<T>>();

        await repo.SaveBulkAsync(outerRecordsLst, accumulator);
    }
}
