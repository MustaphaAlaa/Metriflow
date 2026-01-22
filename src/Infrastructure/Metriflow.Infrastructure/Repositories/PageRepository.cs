using IRepository.Generic;
using Metriflow.Domain.Entities;
using Metriflow.Domain.Entities.Reports;
using Metriflow.Domain.enums;
using Metriflow.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Repositories.Generic;

public class PageRepository(MetriflowDbContext context, ILogger<PageRepository> logger)
    : BaseRepository<Page>(context), IPageRepository
{
    protected readonly MetriflowDbContext _db = context;

    public async Task<List<PageReport>> PageReportAsync()
    {
        var pageReports = await _db
            .PageAnalytics.Include(r => r.Page)
            .GroupBy(r => new { r.PageId, r.Page.Path })
            .Select(g => new PageReport
            {
                Path = g.Key.Path.ToString(),
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
            page.Path == (enPages)combinedAnalyticsMessage.Page
        );
        if (page is null)
        {
            logger.LogInformation(
                $"Creating Page: {combinedAnalyticsMessage.Page} --- Ticks: {combinedAnalyticsMessage.Ticks}"
            );
            page = await CreateAsync(
                new Page { Path = (enPages)combinedAnalyticsMessage.Page  }
            );
        }

        return page;
    }
}