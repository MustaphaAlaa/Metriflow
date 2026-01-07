using IRepository.Generic;
using Metriflow.Domain.Entities;
using Metriflow.Domain.Entities.Reports;
using Metriflow.Domain.enums; 
using Metriflow.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Repositories.Generic;

public class PageRepository : BaseRepository<Page>, IPageRepository
{
    protected readonly MetriflowDbContext _db;
    private readonly ILogger<PageRepository> _logger;

    public PageRepository(MetriflowDbContext context, ILogger<PageRepository> logger)
        : base(context)
    {
        _db = context;
        _logger = logger;
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

    public async Task<Page> GetOrCreatePageAsync(CombinedAnalyticsMessage combinedAnalyticsMessage)
    {
        var page = await RetrieveAsync(page =>
            page.Path == ((enPages)combinedAnalyticsMessage.Page).ToString()
        );
        if (page is null)
        {
            _logger.LogInformation(
                $"Creating Page: {combinedAnalyticsMessage.Page} --- Date: {combinedAnalyticsMessage.Date}"
            );
            page = await CreateAsync(
                new Page { Path = ((enPages)combinedAnalyticsMessage.Page).ToString() }
            );
        }
        return page;
    }
}
