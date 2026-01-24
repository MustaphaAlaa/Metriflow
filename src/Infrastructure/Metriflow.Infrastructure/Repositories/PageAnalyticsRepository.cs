using IRepository.Generic;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Metriflow.Domain.Entities.Reports;
using Metriflow.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.Internal;

namespace Repositories.Generic;

[ServiceRegistration(ServiceLifetime.Scoped, typeof(IPageAnalyticsRepository))]
public class PageAnalyticsRepository(MetriflowDbContext context) : BaseRepository<PageAnalytics>(context)
    , IPageAnalyticsRepository
{
    protected readonly MetriflowDbContext _db = context;

    public IQueryable<PageAnalytics> GetUnaggregatedPageAnalytics( )
    {
        
        var query = from ap in _db.AggregationProgresses
            join pa in _db.PageAnalytics on new { ap.Date, ap.PageId } equals new { pa.Date, pa.PageId }
            select new PageAnalytics()
            {
                Id = pa.Id,
                Date = pa.Date,
                PageId = pa.PageId,
                Users = pa.Users,
                Views = pa.Views,
                PerformanceScore = pa.PerformanceScore,
                Sessions = pa.Sessions,
                LcpMs = pa.LcpMs,
                Intervals = pa.Intervals,
            };
        return query;
    }
}