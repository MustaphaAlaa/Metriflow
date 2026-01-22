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

    public IQueryable<PageAnalytics> GetUnaggregatedPageAnalytics(List<AggregationKey> aggregateKeys)
    {
        
        
        // var dates = aggregateKeys.Select(ak => ak.Date).Distinct().ToList();
        //
        // var pageIds = aggregateKeys.Select(k => k.PageId).Distinct().ToList();
        //
        //
        // return _db.PageAnalytics
        //     .Where(pa => dates.Contains(pa.Date) && pageIds.Contains(pa.PageId));

        
        
        return null;
    }
}