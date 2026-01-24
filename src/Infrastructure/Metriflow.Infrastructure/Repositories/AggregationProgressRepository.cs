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

 

    public void CorrelationAggregated(AggregationProgress aggregationProgress)
    {
        aggregationProgress.Correlation = true;
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

    public async Task CreateRangeWithKeysAsync(IEnumerable<AggregationKey> keys)
    {
        var keysList = keys.ToList();

        var existingKeys = await Db.AggregationProgresses
            .Select(ap => new AggregationKey
            {
                Date = ap.Date,
                PageId = ap.PageId
            })
            .ToListAsync();

        if (existingKeys.Any())
        {
            var newKeys = keys
                .Where(k => !existingKeys.Any(ek =>
                    ek.Date == k.Date && ek.PageId == k.PageId))
                .ToList();

            var newProgresses = newKeys
                .Select(e => new AggregationProgress
                {
                    Date = e.Date,
                    PageId = e.PageId,
                    Daily = false,
                    Monthly = false,
                    Quarterly = false,
                    Yearly = false,
                    Interval = false,
                    Weekly = false,
                })
                .ToList();


            if (newProgresses.Any())
            {
                await Db.AggregationProgresses.AddRangeAsync(newProgresses);
            }
        }
        else
        {
            var progresses = keysList.Select(k => new AggregationProgress
            {
                Date = k.Date,
                PageId = k.PageId,
                Daily = false,
                Monthly = false,
                Quarterly = false,
                Yearly = false,
                Interval = false,
                Weekly = false,
            });
            await Db.AggregationProgresses.AddRangeAsync(progresses);
        }
    }

    public async Task<List<AggregationKey>> GetUnprocessedKeysAsync()
    {
        return await Db.AggregationProgresses.Where(e =>
            !e.Daily && !e.Interval
                     && !e.Monthly && !e.Quarterly && !e.Yearly
                     && !e.Correlation
        ).Select(k => new AggregationKey()
        {
            Date = k.Date,
            PageId = k.PageId
        }).ToListAsync();
    }

    public IQueryable<AggregateRecordsJoins> GetNoneMonthlyAggregateRecords()
    {
        return GetJoins(Db.AggregationProgresses.Where(e =>
            e.Interval && e.Daily && !e.Monthly
        ));
    }

    public IQueryable<AggregationProgress> GetNoneIntervalsAggregateRecords()
    {
          var query = Db.AggregationProgresses.Where(e => !e.Interval);
          return query;





    }

    public IQueryable<AggregateRecordsJoins> GetNoneCorrelationAggregateRecords()
    {
        return GetJoins(Db.AggregationProgresses.AsNoTracking().Where(e => !e.Correlation));
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
            e.Interval && e.Monthly && !e.Yearly
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
        var qs = from ap in queryable 
            join ga in Db.GARecords
                on new { ap.PageId, Date = ap.Date }
                equals new { ga.PageId, Date = (DateTime)(object)ga.Ticks } 
            join psi in Db.PSIRecords
                on new { ap.PageId, Date = ap.Date }
                equals new { psi.PageId, Date = (DateTime)(object)psi.Ticks }
            select new AggregateRecordsJoins
            {
                AggregationProgress = ap,
                Date = ap.Date,
                PageId = ap.PageId,
                GARecord = ga,
                PSIRecord = psi
            };

        return qs;
    }
}