using Metriflow.Domain.Entities;
using Metriflow.DTOs;

namespace IRepository.Generic;

public interface IDailyStatRepository : IBaseRepository<DailyStat>
{
    Task<OverviewReportDto> StatsOverviewAsync();
}
