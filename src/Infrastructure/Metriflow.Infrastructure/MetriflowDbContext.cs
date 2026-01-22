using Metriflow.Domain.Entities;
using Metriflow.Domain.Entities.Workers;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure;

public partial class MetriflowDbContext : DbContext
{
    public MetriflowDbContext(DbContextOptions<MetriflowDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        Console.WriteLine("Constructed");
        Console.WriteLine("Configurations");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MetriflowDbContext).Assembly); 
        // modelBuilder.Entity<AggregationProgress>().HasNoKey();
    }
}