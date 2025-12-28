using IRepository.Generic;
using Metriflow.Domain.Entities;
using Metriflow.DTOs;
using Metriflow.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Generic;

public class DailyStatRepository : BaseRepository<DailyStat>, IDailyStatRepository
{
    protected readonly MetriflowDbContext _db;

    public DailyStatRepository(MetriflowDbContext context)
        : base(context)
    {
        _db = context;
    }

    public async Task<List<PageReport>> PageReportAsync()
    {
        var pageReports = await _db
            .RawDatas.Include(r => r.Page)
            .GroupBy(r => new { r.PageId, r.Page.Path })
            .Select(g => new PageReport
            {
                Path = g.Key.Path,
                TotalUsers = g.Sum(x => x.Users),
                TotalSessions = g.Sum(x => x.Sessions),
                TotalViews = g.Sum(x => x.Views),
                AvgPerformance = g.Average(x => x.PerformanceScore),
            })
            .ToListAsync();

        return pageReports;
    }

    public Task<OverviewReport> StatsOverviewAsync()
    {
        var overview = _db
            .RawDatas.GroupBy(r => 1)
            .Select(g => new OverviewReport
            {
                TotalUsers = g.Sum(x => x.Users),
                TotalSessions = g.Sum(x => x.Sessions),
                TotalViews = g.Sum(x => x.Views),
                AvgPerformance = g.Average(x => x.PerformanceScore),
            })
            .FirstOrDefaultAsync();

        return overview;
    }
}
