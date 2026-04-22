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
            logger.LogWarning("%%%%%%%% Raw Data Worker is Done. %%%%%%%%%%");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "!!!!!!!! Raw Data Worker failed");
           throw new Exception("!!!!!! Raw Data Worker failed", ex);
        }
    }
}