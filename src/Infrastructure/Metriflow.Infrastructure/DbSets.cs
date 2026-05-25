using Metriflow.Domain.Entities;
using Metriflow.Domain.Entities.Workers;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure;

public partial class MetriflowDbContext
{
    public DbSet<Page> Pages { get; set; }
    public DbSet<PageAnalytics> PageAnalytics { get; set; }
    public DbSet<TimeIntervalAnalytic> TimeIntervalsAnalytics { get; set; }
    public DbSet<DailyAnalytics> DailyAnalytics { get; set; }
    public DbSet<TableRowsCount> TableRowsCounts { get; set; }
    public DbSet<MonthlyAnalytic> MonthlyAnalytics { get; set; }
    public DbSet<YearlyAnalytics> YearlyAnalytics { get; set; }
    public DbSet<StagingReadiness> StagingReadiness { get; set; }

    public DbSet<AggregationProgress> AggregationProgresses { get; set; }
    public DbSet<GARecord> GARecords { get; set; }
    public DbSet<PSARecord> PSARecords { get; set; }
    public DbSet<TimeInterval> TimeIntervals { get; set; }
}