using Metriflow.Application.Interfaces;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.Services;

[ServiceRegistration(ServiceLifetime.Scoped, typeof(IDailyAnalyticsService))]
public class DailyAnalyticsService : IDailyAnalyticsService
{
    private readonly ILogger<DailyAnalyticsService> _logger;

    public DailyAnalyticsService(ILogger<DailyAnalyticsService> logger)
    {
        _logger = logger;
    }

    public async Task<DailyAnalytics> CalculateDailyStat(
        List<PageAnalytics> pages
    )
    {

     if(pages.Count == 0 || pages.Contains(null))
       throw new NullReferenceException("pages for DailyAnalytic is null, in DailyAnalyticsService");


        var dailyAnalytics = new DailyAnalytics
        {
            Date = pages[0].Date.Date,
            PageId = pages[0].PageId,
            ReceivedAt = DateTime.UtcNow,
        };

        AggregateUtilities.Aggregate(dailyAnalytics, pages);
        return dailyAnalytics;
    }
}