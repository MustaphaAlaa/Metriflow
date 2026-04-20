using IRepository.Generic;
using Metriflow.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.Services.Orchestration;

public class PageAnalyticsOrchestration(
    IAggregationProgressRepository aggregationProgressRepository,
    IPageAnalyticsRepository pageAnalyticsRepository,
    IPageAnalyticsServices pageAnalyticService,
    ILogger<PageAnalyticsOrchestration> logger
) : IPageAnalyticsOrchestration

{
    public async Task<int> CreatePageAnalyticsAsync()
    {
        try
        {
            logger.LogInformation("@@@@@@@@@PageAnalyticsOrchestration PageAnalytics Creating should be start");

            await pageAnalyticsRepository.CorrlelationAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        } 
       
        return 9999;
    }
}