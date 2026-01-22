using Metriflow.AggregationWorker.Services;

namespace Metriflow.AggregationWorker.Workers;

public class IntervalAnalyticsWorker(ILogger<IntervalAnalyticsWorker> logger )
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    { 
        logger.LogInformation("Interval Analytics Worker is starting");
        // await Task.WhenAll(workerConsumer.Consume(stoppingToken));
    }
}