using Metriflow.Domain.Entities;
using Metriflow.Domain.Entities.Reports;

namespace IRepository.Generic;

public interface IDailyAnalyticsRepository : IBaseRepository<DailyAnalytics>
{
    Task<OverviewReport> StatsOverviewAsync();
}
