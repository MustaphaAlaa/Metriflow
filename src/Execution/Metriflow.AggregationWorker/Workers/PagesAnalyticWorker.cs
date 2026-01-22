using Metriflow.Application.Entities;
using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Workers;

namespace Metriflow.AggregationWorker.Workers;

public class PagesAnalyticWorker(
    ILogger<DailyAnalyticsWorker> logger,
    IProducer producer,
    IPageAnalyticsOrchestration pageAnalyticsOrchestration,
    IConfigurationManager confg) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Page analytics worker is started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var pagesCount = await pageAnalyticsOrchestration.CreatePageAnalyticsAsync();

            await producer.NotifyCompletedMessage(new AggregationCompletedMessage()
                {
                    CorrelationId = Guid.NewGuid(),
                    CompletedAt = DateTime.Now,
                    CompletedType = AggregationType.Page,
                    ProcessedCount = pagesCount
                }, confg.GetSection("Queues:IntervalAggregation").Value,
                "a"
            );


            await Task.Delay(2000, stoppingToken);
        }
    }
}