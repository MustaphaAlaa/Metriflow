using Metriflow.Application.interfaces;
using Metriflow.Domain.Entities;
using Metriflow.DTOs;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.Services;

public class DailyStateCalculator : IDailyStatCalculator
{
    private readonly ILogger<DailyStateCalculator> _logger;

    public DailyStateCalculator(ILogger<DailyStateCalculator> logger)
    {
        _logger = logger;
    }

    public async Task<DailyStat> CalculateDailyStat(
        List<CombinedAnalyticsMessage> combinedAnalyticsMessages
    )
    {
        var tm = new TimeOnly(0, 0);
        return new DailyStat
        {
            TotalUsers = combinedAnalyticsMessages.Sum(r => r.Users),
            TotalViews = combinedAnalyticsMessages.Sum(r => r.Views),
            TotalSessions = combinedAnalyticsMessages.Sum(r => r.Sessions),
            AvgPerformance = combinedAnalyticsMessages.Average(rc => rc.PerformanceScore),
            ReceivedAt = DateTime.UtcNow,
            Date = combinedAnalyticsMessages[0].Date.ToDateTime(tm),
        };
    }
}
