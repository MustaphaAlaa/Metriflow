using Metriflow.Domain.Entities;

namespace Metriflow.Application.Interfaces;

public interface IDailyAnalyticsService
{
    Task<DailyAnalytics> CalculateDailyStat(List<PageAnalytics> pages);
}