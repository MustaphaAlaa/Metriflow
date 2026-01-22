using Metriflow.AggregationWorker.Services;

namespace Metriflow.AggregationWorker.Workers;

public class MonthlyAnalyticsWorker(ILogger<MonthlyAnalyticsWorker> logger) : BackgroundService
{
    private readonly ILogger<MonthlyAnalyticsWorker> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    { 
        // await Task.WhenAll(_workerConsumer.Consume(stoppingToken));
    }
}