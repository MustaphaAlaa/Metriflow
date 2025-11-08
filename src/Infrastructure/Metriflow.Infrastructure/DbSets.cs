using Metriflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure;

public partial class MetriflowDbContext
{
    public DbSet<Page> Pages { get; set; }
    public DbSet<DailyStat> DailyStats { get; set; } 
    public DbSet<RawData> RawDatas { get; set; }
}