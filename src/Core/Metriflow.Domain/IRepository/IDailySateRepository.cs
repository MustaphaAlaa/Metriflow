using Metriflow.Domain.Entities;
using Metriflow.Domain.Entities.Reports;

namespace IRepository.Generic;

public interface IDailyStatRepository : IBaseRepository<DailyStat>
{
    Task<OverviewReport> StatsOverviewAsync();
}
