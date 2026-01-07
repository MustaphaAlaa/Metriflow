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
        
        return new DailyAnalytics
        {
            TotalUsers = combinedAnalyticsMessages.Sum(r => r.Users),
            TotalViews = combinedAnalyticsMessages.Sum(r => r.Views),
            TotalSessions = combinedAnalyticsMessages.Sum(r => r.Sessions),
            AvgPerformance = combinedAnalyticsMessages.Average(rc => rc.PerformanceScore),
            ReceivedAt = DateTime.UtcNow,
            //!!!! Date = combinedAnalyticsMessages[0].Date ,
        };
    }
}
