using IRepository.Generic;
using Metriflow.Application.Interfaces;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.Services.AggregationServices.Orchestration;

[ServiceRegistration(ServiceLifetime.Scoped, typeof(IMonthlyAnalyticsOrchestrator))]
public class MonthlyAnalyticsOrchestrator(
    IAggregationProgressRepository aggregationProgressRepository,
    IPageAnalyticsRepository pageAnalyticsRepository,
    IMonthlyAnalyticService monthlyAnalyticService,
    IBaseRepository<MonthlyAnalytic> monthlyAnalyticsRepository,
    ILogger<MonthlyAnalyticsOrchestrator> logger)
    : IMonthlyAnalyticsOrchestrator
{
    public async Task<int> AggregateMonthlyAnalyticsAsync()
    {
        try
        {
            var unprocessedRecords = pageAnalyticsRepository.GetUnaggregatedMonthlyPageAnalytics().GroupBy(pa => new { pa.Date.Year, pa.Date.Month, pa.PageId }).ToDictionary(pa => pa.Key, pa => pa.Select(p => p));

            if (!unprocessedRecords.Any())
            {
                logger.LogInformation("@@@@@@@No unprocessed keys found for monthly aggregation");
                return 0;
            }

            logger.LogInformation("$$$$$$Found {Count} unprocessed records for monthly aggregation", unprocessedRecords.Count);

            var chunkCount = 500;
            var recordsCount = 0;

            foreach (var chunk in unprocessedRecords.Chunk(chunkCount))
            {
                foreach (var group in chunk)
                {
                    var pages = group.Value.ToList();
                    if (pages == null)
                        throw new NullReferenceException("$$$$$$$pages for MonthlyAnalytic is null, in MonthlyAnalyticsOrchestrator");
                    
                    var newMonthlyAnalytics = monthlyAnalyticService.NormalizeMonthlyAnalytic(pages);

                    if (newMonthlyAnalytics == null)
                        throw new NullReferenceException("$$$$$Normalized MonthlyAnalytic is null, in MonthlyAnalyticsOrchestrator");

                    await monthlyAnalyticsRepository.CreateAsync(newMonthlyAnalytics);

                    foreach (var page in pages)
                    {
                        if (page == null)
                            throw new NullReferenceException("$$$$$PageAnalytics record is null, in MonthlyAnalyticsOrchestrator");

                        var ap = await aggregationProgressRepository.RetrieveTrackedAsync(ap =>
                            ap.Date == page.Date && ap.PageId == page.PageId);
                        if (ap is null)
                            throw new NullReferenceException($"AggregationProgress record is null for Date: {page.Date}, PageId: {page.PageId}");

                        aggregationProgressRepository.MonthlyAggregated(ap);
                        aggregationProgressRepository.Update(ap);
                    }
                    recordsCount++;
                }

                await monthlyAnalyticsRepository.SaveChangesAsync();
                logger.LogInformation("$$$$$$$$$$Successfully aggregated records to MonthlyAnalytics {Count}", recordsCount);
            }

            logger.LogInformation("$$$$$$$$$Total Successfully aggregated records to MonthlyAnalytics {Count}", recordsCount);
            return recordsCount;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to aggregate monthly analytics");
            throw;
        }
    }
}
