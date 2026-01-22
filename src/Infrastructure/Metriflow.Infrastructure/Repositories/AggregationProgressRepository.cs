using IRepository.Generic;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Metriflow.Domain.Entities.Workers;
using Metriflow.Domain.Interfaces;
using Metriflow.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Repositories.Generic;

[ServiceRegistration(lifetime: ServiceLifetime.Scoped, typeof(IAggregationProgressRepository))]
public class AggregationProgressRepository : BaseRepository<AggregationProgress>, IAggregationProgressRepository
{
    private readonly MetriflowDbContext Db;

    public AggregationProgressRepository(MetriflowDbContext context) : base(context)
    {
        Db = context;
    }

    public void IntervalAggregated(AggregationProgress aggregationProgress)
    {
        aggregationProgress.Interval = true;
    }

    public void DailyAggregated(AggregationProgress aggregationProgress)
    {
        aggregationProgress.Daily = true;
    }

    public void MonthlyAggregated(AggregationProgress aggregationProgress)
    {
        aggregationProgress.Monthly = true;
    }

    public void YearlyAggregated(AggregationProgress aggregationProgress)
    {
        aggregationProgress.Yearly = true;
    }

    public void QuarterlyAggregated(AggregationProgress aggregationProgress)
    {
        aggregationProgress.Quarterly = true;
    }

    

    public async Task<List<AggregationKey>> GetUnprocessedKeysAsync() 
    {
        return await Db.AggregationProgresses.Where(e =>
            !e.Daily && !e.Interval
                     && !e.Monthly && !e.Quarterly && !e.Yearly
        ).Select(k=> new AggregationKey()
        {
            Date = k.Date,
            PageId = k.PageId
        }).ToListAsync();
    }

    public IQueryable<AggregateRecordsJoins> GetNoneMonthlyAggregateRecords()  
    {
        return GetJoins(Db.AggregationProgresses.Where(e =>
            e.Interval&& e.Daily && !e.Monthly
        ));
    }

    public IQueryable<AggregateRecordsJoins> GetNoneIntervalsAggregateRecords()
    {
        return GetJoins(Db.AggregationProgresses.Where(e=> !e.Interval));
    }

    public IQueryable<AggregateRecordsJoins> GetNoneDailyAggregateRecords()  
    {
        return GetJoins(Db.AggregationProgresses.Where(e =>
            e.Interval && !e.Daily
        ));
    }
    public IQueryable<AggregateRecordsJoins> GetNoneYearlyAggregateRecords()  
    {
        return GetJoins(Db.AggregationProgresses.Where(e =>
            e.Interval  && e.Monthly && !e.Yearly
        ));
    }
    public IQueryable<AggregateRecordsJoins> GetNoneQueryableAggregateRecords()  
    {
        return GetJoins(Db.AggregationProgresses.Where(e =>
            e.Interval && e.Monthly && !e.Quarterly
        ));
    }

    private IQueryable<AggregateRecordsJoins> GetJoins(IQueryable<AggregationProgress> queryable)
    {
        return queryable
            .Join(Db.GARecords,
                ap => new { ap.Date, PageId = ap.PageId },
                ga => new { Date = new DateTime(ga.Ticks), PageId = ga.Page },
                (ap, ga) => new AggregateRecordsJoins() { Date = ap.Date, PageId = ga.Page, GARecord = ga })
            .Join(Db.PSIRecords,
                ap => new { Date = ap.Date, PageId = ap.PageId }
                ,
                psi => new { Date = new DateTime(psi.Ticks), PageId = psi.Page },
                (ap, psi) => new AggregateRecordsJoins()
                    { Date = ap.Date, PageId = ap.PageId, GARecord = ap.GARecord, PSIRecord = psi });
    }
}