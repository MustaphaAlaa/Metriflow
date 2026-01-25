using IRepository.Generic;
using Metriflow.Application.Interfaces;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Metriflow.AggregationWorker.Services;

[ServiceRegistration(ServiceLifetime.Scoped, typeof(IDailyAnalyticsOrchestrator))]
public class DailyAnalyticsOrchestrator(
    IAggregationProgressRepository aggregationProgressRepository,
    IPageAnalyticsRepository pageAnalyticsRepository,
    IDailyAnalyticsService dailyAnalyticsService,
    IDailyAnalyticsRepository dailyAnalyticsRepository,
    ILogger<DailyAnalyticsOrchestrator> logger)
    : IDailyAnalyticsOrchestrator
{
    public async Task<int> AggregateDailyAnalyticsAsync()
    {

            var unprocessedRecords = pageAnalyticsRepository.GetUnaggregatedDailyPageAnalytics().GroupBy(pa => new { pa.Date.Day, pa.PageId}).ToDictionary(pa => pa.Key, pa => pa.Select(p => p)
                );


              
                var chunkCount = 500;
                var recordsCount = 0;

                foreach (var chunk in unprocessedRecords.Chunk(chunkCount))
                {
                    foreach (var group in chunk)
                    {
                        var pages = group.Value.ToList();
                        if (pages == null)
                            throw new NullReferenceException("$$$$$$$pages for DailyAnalytic is null, in DailyAnalyticsOrchestrator");
                        var newDailiyDailyAnalytics = await dailyAnalyticsService.CalculateDailyStat(pages);

                        if (newDailiyDailyAnalytics == null)
                            throw new NullReferenceException("$$$$$Normalized DailyAnalytic is null, in DailyAnalyticsOrchestrator");

                        await dailyAnalyticsRepository.CreateAsync(newDailiyDailyAnalytics);


                        foreach (var page in pages)
                        {
                            if (page == null)
                                throw new NullReferenceException("$$$$$PageAnalytics record is null, in DailyAnalyticsOrchestrator");

                            var ap = await aggregationProgressRepository.RetrieveTrackedAsync(ap =>
                                ap.Date == page.Date && ap.PageId == page.PageId);
                            if (ap is null)
                                throw new NullReferenceException($"AggregationProgress record is null for Date: {page.Date}, PageId: {page.PageId}");

                            aggregationProgressRepository.DailyAggregated(ap);
                            aggregationProgressRepository.Update(ap);
                        }
                        recordsCount++;

                    }

                    await dailyAnalyticsRepository.SaveChangesAsync();
                    logger.LogInformation("$$$$$$$$$$Successfully aggregated records to DailyAnalytics {Count}", recordsCount);

                }
                logger.LogInformation("$$$$$$$$$Total Successfully aggregated records to DailyAnalytics {Count}", recordsCount);
                
                return recordsCount;
 
    }


}