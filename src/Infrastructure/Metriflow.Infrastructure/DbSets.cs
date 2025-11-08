using Metriflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure;

public partial class MetriflowDbContext
{
    public DbSet<Page> Pages;
    public DbSet<DailyStat> DailyStats;
    public DbSet<MonthlyStat> MonthlyStats;
    public DbSet<YearlyStat> YearlyStats;
    public DbSet<RawData> RawDates;
}
