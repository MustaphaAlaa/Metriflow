namespace Metriflow.AggregationWorker.Workers;

public class AggregationProgressWorker(ILogger<AggregationProgressWorker> logger) : BackgroundService
{
    private readonly ILogger<AggregationProgressWorker> _logger = logger;


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
       
    }
}