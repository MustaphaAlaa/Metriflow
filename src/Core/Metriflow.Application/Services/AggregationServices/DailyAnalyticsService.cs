using Metriflow.Application.Interfaces;
using Metriflow.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.Services;

public class DailyAnalyticsService : IDailyAnalyticsService
{
    private readonly ILogger<DailyAnalyticsService> _logger;

    public DailyAnalyticsService(ILogger<DailyAnalyticsService> logger)
    {
        _logger = logger;
    }

    public async Task<DailyAnalytics> CalculateDailyStat(
        List<CombinedAnalyticsMessage> combinedAnalyticsMessages
    )
    {
        //@@ Refactor
        //It should be from TimeIntervalAnalytics 
        var dailyAnalytics = new DailyAnalytics
        {
                     // ReceivedAt = DateTime.UtcNow,
             //!!!! Ticks = combinedAnalyticsMessages[0].Ticks ,
        };

        // AggregateUtilities.Aggregate(dailyAnalytics, combinedAnalyticsMessages);
        return dailyAnalytics;
    }
}
