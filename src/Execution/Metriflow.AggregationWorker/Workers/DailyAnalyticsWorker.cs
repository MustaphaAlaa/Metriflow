using Metriflow.AggregationWorker.Services;
using Metriflow.Domain.CustomAttributes;

namespace Metriflow.AggregationWorker.Workers;
 
public class DailyAnalyticsWorker(ILogger<DailyAnalyticsWorker> logger) : BackgroundService
{
    private readonly ILogger<DailyAnalyticsWorker> _logger = logger;


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // await Task.WhenAll(_workerConsumer.Consume(stoppingToken));
    }
}