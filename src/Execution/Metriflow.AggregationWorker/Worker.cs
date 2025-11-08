using Metriflow.AggregationWorker.Services;

namespace Metriflow.AggregationWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    private readonly IAggregationWorkerConsumer _workerConsumer;

    public Worker(ILogger<Worker> logger, IAggregationWorkerConsumer workerConsumer)
    {
        _workerConsumer = workerConsumer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // while (!stoppingToken.IsCancellationRequested)
        // {
        //     if (_logger.IsEnabled(LogLevel.Information))
        //     {
        //         _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        //     }
        //     await Task.Delay(1000, stoppingToken);
        // }
        await Task.WhenAll(_workerConsumer.Consume(stoppingToken));
    }
}
