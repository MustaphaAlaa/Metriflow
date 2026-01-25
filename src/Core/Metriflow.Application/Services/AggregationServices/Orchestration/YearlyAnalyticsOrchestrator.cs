using IRepository.Generic;
using Metriflow.Application.Interfaces;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.Services.AggregationServices.Orchestration;

[ServiceRegistration(ServiceLifetime.Scoped, typeof(IYearlyAnalyticsOrchestrator))]
public class YearlyAnalyticsOrchestrator(
    IAggregationProgressRepository aggregationProgressRepository,
    IPageAnalyticsRepository pageAnalyticsRepository,
    IYearAnalyticService yearAnalyticService,
    IBaseRepository<MonthlyAnalytic> monthlyAnalyticsRepository,
    IBaseRepository<YearlyAnalytics> yearlyAnalyticsRepository,
    ILogger<YearlyAnalyticsOrchestrator> logger)
    : IYearlyAnalyticsOrchestrator
{
    public async Task<int> AggregateYearlyAnalyticsAsync()
    {
        try
        {
            var unprocessedRecords = pageAnalyticsRepository.GetUnaggregatedYearlyPageAnalytics().GroupBy(pa => new { pa.Date.Year, pa.PageId }).ToDictionary(pa => pa.Key, pa => pa.Select(p => p));

            if (!unprocessedRecords.Any())
            {
                logger.LogInformation("@@@@@@@No unprocessed keys found for yearly aggregation");
                return 0;
            }

            logger.LogInformation("$$$$$$Found {Count} unprocessed records for yearly aggregation", unprocessedRecords.Count);

            var chunkCount = 500;
            var recordsCount = 0;

            foreach (var chunk in unprocessedRecords.Chunk(chunkCount))
            {
                foreach (var group in chunk)
                {
                    var pages = group.Value.ToList();
                    if (pages == null)
                        throw new NullReferenceException("$$$$$$$pages for YearlyAnalytics is null, in YearlyAnalyticsOrchestrator");

                    // Group by month for yearly calculation
                    var monthlyGroupedData = pages
                        .GroupBy(p => new { p.Date.Year, p.Date.Month })
                        .Select(g => new MonthlyAnalytic
                        {
                            PageId = pages.First().PageId,
                            YearMonth = new DateTime(g.Key.Year, g.Key.Month, 1),
                            TotalUsers = g.Sum(p => p.Users),
                            TotalSessions = g.Sum(p => p.Sessions),
                            TotalViews = g.Sum(p => p.Views),
                            AvgPerformance = g.Average(p => p.PerformanceScore)
                        })
                        .ToList();

                    var newYearlyAnalytics = yearAnalyticService.NormalizeYearlyAnalytic(monthlyGroupedData);

                    if (newYearlyAnalytics == null)
                        throw new NullReferenceException("$$$$$Normalized YearlyAnalytics is null, in YearlyAnalyticsOrchestrator");

                    await yearlyAnalyticsRepository.CreateAsync(newYearlyAnalytics);

                    foreach (var page in pages)
                    {
                        if (page == null)
                            throw new NullReferenceException("$$$$$PageAnalytics record is null, in YearlyAnalyticsOrchestrator");

                        var ap = await aggregationProgressRepository.RetrieveTrackedAsync(ap =>
                            ap.Date == page.Date && ap.PageId == page.PageId);
                        if (ap is null)
                            throw new NullReferenceException($"AggregationProgress record is null for Date: {page.Date}, PageId: {page.PageId}");

                        aggregationProgressRepository.YearlyAggregated(ap);
                        aggregationProgressRepository.Update(ap);
                    }
                    recordsCount++;
                }

                await yearlyAnalyticsRepository.SaveChangesAsync();
                logger.LogInformation("$$$$$$$$$$Successfully aggregated records to YearlyAnalytics {Count}", recordsCount);
            }

            logger.LogInformation("$$$$$$$$$Total Successfully aggregated records to YearlyAnalytics {Count}", recordsCount);
            return recordsCount;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to aggregate yearly analytics");
            throw;
        }
    }
}
