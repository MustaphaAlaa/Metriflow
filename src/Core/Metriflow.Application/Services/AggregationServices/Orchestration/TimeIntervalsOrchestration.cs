using System.Globalization;
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
    public async Task AggregateTimeIntervalsAsync()
    {
        try
        {
            var unprocessedKeys = await aggregationProgressRepository.GetUnprocessedKeysAsync();

            if (!unprocessedKeys.Any())
            {
                logger.LogInformation("No unprocessed keys found");
                return;
            }

            logger.LogInformation("Processing {Count} unprocessed keys", unprocessedKeys.Count);

            var dates = unprocessedKeys.Select(k => k.Date).Distinct().ToList();
            var pageIds = unprocessedKeys.Select(k => k.PageId).Distinct().ToList();

            var pages = await pageAnalyticsRepository.RetrieveAllAsync(pa =>
                dates.Contains(pa.Date) && pageIds.Contains(pa.PageId)
            );

            var filteredPages = pages.GroupBy(p => new { p.Date, p.PageId, p.Intervals });

            var timeIntervalAnalytics = await _timeIntervalAnalyticsRepository.RetrieveAllAsync(ti =>
                dates.Contains(ti.Date) && pageIds.Contains(ti.PageId));

            var progressRecords = await aggregationProgressRepository.RetrieveAllAsync(ag =>
                dates.Contains(ag.Date) && pageIds.Contains(ag.PageId));

            var intervalAnalyticsDictionary =
                timeIntervalAnalytics.ToDictionary(t => (t.Date, t.PageId, t.TimeIntervalId));

            var aggregationProgressesDictionary = progressRecords.ToDictionary(p => (p.Date, p.PageId));


            foreach (var group in filteredPages)
            {
                var newTimeInterval = _timeIntervalAnalyticService.NormalizeTimeIntervalAnalytic(group.ToList());

                var key = (group.Key.Date, group.Key.PageId, (byte)group.Key.Intervals);
                if (!intervalAnalyticsDictionary.TryGetValue(key, out var existingTimeInterval))
                {
                    await
                        _timeIntervalAnalyticsRepository.CreateAsync(newTimeInterval);
                }
                else
                {
                    existingTimeInterval.AvgPerformance += newTimeInterval.AvgPerformance;
                    existingTimeInterval.TotalSessions += newTimeInterval.TotalSessions;
                    existingTimeInterval.TotalUsers += newTimeInterval.TotalUsers;
                    existingTimeInterval.TotalViews += newTimeInterval.TotalViews;

                    _timeIntervalAnalyticsRepository.Update(existingTimeInterval);
                }

                if (aggregationProgressesDictionary.TryGetValue((group.Key.Date, group.Key.PageId), out var agp))
                {
                    aggregationProgressRepository.IntervalAggregated(
                        agp);
                }
            }

            await _timeIntervalAnalyticsRepository.SaveChangesAsync();
            logger.LogInformation("Successfully processed {Count} groups", filteredPages.Count());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to aggregate time intervals");
            throw;
        }
    }
}