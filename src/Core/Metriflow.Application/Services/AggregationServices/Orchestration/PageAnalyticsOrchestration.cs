using IRepository.Generic;
using Metriflow.Application.Interfaces;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Metriflow.Domain.Entities.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.Services.Orchestration;

public class PageAnalyticsOrchestration(
    IAggregationProgressRepository aggregationProgressRepository,
    IBaseRepository<PageAnalytics> pageAnalyticsRepository,
    IPageAnalyticsServices pageAnalyticService,
    ILogger<PageAnalyticsOrchestration> logger
) : IPageAnalyticsOrchestration

{
    public async Task<int> CreatePageAnalyticsAsync()
    {
        var unprocessedKeys = aggregationProgressRepository.GetNoneCorrelationAggregateRecords().ToList();

        if (!unprocessedKeys.Any())
            return 0;

        logger.LogInformation("@@@@@@@@@PageAnalyticsOrchestration PageAnalytics Creating should be start");


        var pagesAnalytics = pageAnalyticService.RecordsToPageAnalytics(unprocessedKeys);

        await pageAnalyticsRepository.CreateRangeAsync(pagesAnalytics);
        logger.LogInformation("@@@@@@@@@@@@PageAnalyticsOrchestration PageAnalytics Creating  should be finished");
        logger.LogInformation($"@@@@@@@@@@@PageAnalytics count :{pagesAnalytics.Count}");

        var aggregationProgresses = unprocessedKeys.Select(r => r.AggregationProgress);
        foreach (var aggregationProgress in aggregationProgresses)
            aggregationProgressRepository.CorrelationAggregated(aggregationProgress);

        logger.LogInformation("@@@@@@@@@@@aggregation progress should be start updating");

        aggregationProgressRepository.UpdateRange(aggregationProgresses);

        logger.LogInformation("@@@@@@@@@@@aggregation progress should be start updated");
        await pageAnalyticsRepository.SaveChangesAsync();
        return pagesAnalytics.Count;
    }
}