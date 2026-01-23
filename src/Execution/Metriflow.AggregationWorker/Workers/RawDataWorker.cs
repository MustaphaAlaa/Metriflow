using Metriflow.AggregationWorker.Interfaces;
using Metriflow.AggregationWorker.Interfaces.Correlation;
using Metriflow.Application.Interfaces;

namespace Metriflow.AggregationWorker.Workers;

public class RawDataWorker(
    IRawDataConsumer rawDataConsumer,
    ILogger<RawDataWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Raw Data Worker started");

        try
        {
            rawDataConsumer.Consume(stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Raw Data Worker failed");
            throw;
        }
    }
}