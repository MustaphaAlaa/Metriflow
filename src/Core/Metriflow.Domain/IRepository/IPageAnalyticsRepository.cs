using Metriflow.Domain.Entities;

namespace IRepository.Generic;

public interface IPageAnalyticsRepository : IBaseRepository<PageAnalytics>
{
    IQueryable<PageAnalytics> GetUnaggregatedIntervalsPageAnalytics();
    IQueryable<PageAnalytics> GetUnaggregatedDailyPageAnalytics();
    IQueryable<PageAnalytics> GetUnaggregatedMonthlyPageAnalytics();
    IQueryable<PageAnalytics> GetUnaggregatedYearlyPageAnalytics();
    Task<int> CorrlelationAsync();
}