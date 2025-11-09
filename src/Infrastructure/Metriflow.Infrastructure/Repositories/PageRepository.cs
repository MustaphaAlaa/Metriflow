using IRepository.Generic;
using Metriflow.Domain.Entities;
using Metriflow.DTOs;
using Metriflow.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Generic;

public class PageRepository : BaseRepository<Page>, IPageRepository
{
    protected readonly MetriflowDbContext _db;

    public PageRepository(MetriflowDbContext context)
        : base(context)
    {
        _db = context;
    }

    public async Task<List<PageReportDto>> PageReportAsync()
    {
        var pageReports = await _db
            .RawDatas.Include(r => r.Page)
            .GroupBy(r => new { r.PageId, r.Page.Path })
            .Select(g => new PageReportDto
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
}
