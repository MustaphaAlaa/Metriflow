using System.Linq.Expressions;
using IRepository;
using IRepository.Generic;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Metriflow.Domain.Entities.Reports;
using Metriflow.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Repositories.Generic;

[ServiceRegistration(ServiceLifetime.Scoped, typeof(IPageAnalyticsRepository))]
public class PageAnalyticsRepository(MetriflowDbContext context, ITrackTableCountRepository trackTableCountRepository)
    : BaseRepository<PageAnalytics>(context), IPageAnalyticsRepository
{
    protected readonly MetriflowDbContext _db = context;

    public IQueryable<PageAnalytics> GetUnaggregatedIntervalsPageAnalytics()
    {
        var query = GetConditionalPageAnalyticsQueryable(ap => ap.Interval == false);
        return query;
    }

    public IQueryable<PageAnalytics> GetUnaggregatedDailyPageAnalytics()
    {
        var query = GetConditionalPageAnalyticsQueryable(ap => ap.Daily == false);
        return query;
    }

    public IQueryable<PageAnalytics> GetUnaggregatedMonthlyPageAnalytics()
    {
        var query = GetConditionalPageAnalyticsQueryable(ap => ap.Monthly == false);
        return query;
    }

    public IQueryable<PageAnalytics> GetUnaggregatedYearlyPageAnalytics()
    {
        var query = GetConditionalPageAnalyticsQueryable(ap => ap.Yearly == false);
        return query;
    }

    private IQueryable<PageAnalytics> GetConditionalPageAnalyticsQueryable(
        Expression<Func<AggregationProgress, bool>> predict)
    {
        var filteredAggregations = _db.AggregationProgresses.Where(predict);
        var query = from ap in filteredAggregations
                    join pa in _db.PageAnalytics on new { ap.Date, ap.PageId } equals new { pa.Date, pa.PageId }
                    select new PageAnalytics()
                    {
                        // Id = pa.Id,
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


    public async Task<int> CorrlelationAsync()
    {
        var strategy = _db.Database.CreateExecutionStrategy();

        
        var    insertionCount = 0;


        await strategy.ExecuteAsync(async () =>
        {
            await _db.Database.OpenConnectionAsync();
            var connection = _db.Database.GetDbConnection() as SqlConnection;
            using var transaction = await connection.BeginTransactionAsync();



            var sql = """
            INSERT INTO "PageAnalytics" (
            "PageId", "Date", "Intervals", "Users",
            "Sessions", "Views", "PerformanceScore", "LcpMs"
            )
            SELECT
            pa."PageId",
            pa."Date",
            get_timeinterval(EXTRACT(HOUR FROM pa."Date")),
            ga."Users",
            ga."Sessions",
            ga."Views",
            psi."PerformanceScore",
            psi."LCP_MS"
            FROM (
            "GARecords" AS ga
            JOIN "PSIRecords" AS psi ON ga."PageId" = psi."PageId" AND ga."Date" = psi."Date"
            JOIN "AggregationProgresses" AS pa ON ga."PageId" = pa."PageId" AND ga."Date" = pa."Date"

            )
            Where  exists(
            SELECT 1 FROM
            (
            Select  pa."PageId",  pa."Date" FROM "AggregationProgresses" AS pa
            EXCEPT
            SELECT  aaa."PageId" , aaa."Date" FROM "PageAnalytics" as aaa
            )
            ) and pa."Correlation" = false;


            UPDATE "AggregationProgresses"
            SET "Correlation" = true
            Where (
            "AggregationProgresses"."Correlation" = false
            and  EXISTS (
            SELECT 1
            FROM "PageAnalytics"
            WHERE "AggregationProgresses"."PageId" = "PageAnalytics"."PageId"
            AND "AggregationProgresses"."Date" = "PageAnalytics"."Date"
            )
            );


            UPDATE "AggregationProgresses"
            SET "Correlation" = true
            WHERE "Correlation" = false
            AND EXISTS (
            SELECT 1
            FROM "PageAnalytics"
            WHERE "AggregationProgresses"."PageId" = "PageAnalytics"."PageId"
            AND "AggregationProgresses"."Date" = "PageAnalytics"."Date"
            );
        """;
          

            try
            {
                using var cmd = new SqlCommand(sql, connection);
                cmd.CommandTimeout = 300; // 5 minutes
                
                  insertionCount = await cmd.ExecuteNonQueryAsync();
                // var updateTrackedTable = await trackTableCountRepository.AlterTableRowsCountAsync("PageAnalytics", insertionCount);
                await transaction.CommitAsync();
               
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
         return insertionCount;
    }

}