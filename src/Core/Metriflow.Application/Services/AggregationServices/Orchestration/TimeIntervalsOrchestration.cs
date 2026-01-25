using IRepository.Generic;
using Metriflow.Application.Interfaces;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.Services.Orchestration;

[ServiceRegistration(ServiceLifetime.Scoped, typeof(ITimeIntervalsOrchestration))]
public class TimeIntervalsOrchestration(
    IBaseRepository<TimeIntervalAnalytic> _timeIntervalAnalyticsRepository,
   IAggregationProgressRepository aggregationProgressRepository,
    IPageAnalyticsRepository pageAnalyticsRepository,
    ITimeIntervalAnalyticService _timeIntervalAnalyticService,
    ILogger<TimeIntervalsOrchestration> logger
) : ITimeIntervalsOrchestration
{
    public async Task<int> AggregateTimeIntervalsAsync()
    {
        try
        {
            var unprocessedRecords = pageAnalyticsRepository.GetUnaggregatedIntervalsPageAnalytics().GroupBy(pa => new { pa.Date.Day, pa.PageId, pa.Intervals }).ToDictionary(pa => pa.Key, pa => pa.Select(p => p));

            if (!unprocessedRecords.Any())
            {
                logger.LogInformation("@@@@@@@No unprocessed keys found");
                return 0;
            }

            logger.LogInformation("$$$$$$Found {Count} unprocessed records for time interval aggregation", unprocessedRecords.Count);


            var chunkCount = 500;
            var recordsCount = 0;

            foreach (var chunk in unprocessedRecords.Chunk(chunkCount))
            {
                foreach (var group in chunk)
                {
                    var pages = group.Value.ToList();
                    if (pages == null)
                        throw new NullReferenceException("pages for TimeIntervalAnalytic is null");
                    var newTimeInterval = _timeIntervalAnalyticService.NormalizeTimeIntervalAnalytic(pages);

                    if (newTimeInterval == null)
                        throw new NullReferenceException("Normalized TimeIntervalAnalytic is null");

                    await _timeIntervalAnalyticsRepository.CreateAsync(newTimeInterval);


                    foreach (var page in pages)
                    {
                        if (page == null)
                            throw new NullReferenceException("PageAnalytics record is null");

                        var ap = await aggregationProgressRepository.RetrieveTrackedAsync(ap =>
                            ap.Date == page.Date && ap.PageId == page.PageId);
                        if (ap is null)
                            throw new NullReferenceException($"AggregationProgress record is null for Date: {page.Date}, PageId: {page.PageId}");

                        aggregationProgressRepository.IntervalAggregated(ap);
                        aggregationProgressRepository.Update(ap);
                    }
                        recordsCount++;

                }

                await _timeIntervalAnalyticsRepository.SaveChangesAsync();
                logger.LogInformation("$$$$$$$$$$Successfully aggregated records to TimeIntervalAnalytics {Count}", recordsCount);

            }


            logger.LogInformation("$$$$$$$$$Total Successfully aggregated records to TimeIntervalAnalytics {Count}", recordsCount);
            return recordsCount;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to aggregate time intervals");
            throw;
        }
    }
}