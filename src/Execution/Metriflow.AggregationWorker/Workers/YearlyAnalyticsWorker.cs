using Metriflow.AggregationWorker.Services;

namespace Metriflow.AggregationWorker.Workers;

public class YearlyAnalyticsWorker(ILogger<YearlyAnalyticsWorker> logger )
    : BackgroundService
{
    private readonly ILogger<YearlyAnalyticsWorker> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    { 
        // await Task.WhenAll(workerConsumer.Consume(stoppingToken));
    }
}