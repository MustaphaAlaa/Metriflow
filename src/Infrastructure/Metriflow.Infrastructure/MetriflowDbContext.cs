using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure;

public partial class MetriflowDbContext : DbContext
{
    public MetriflowDbContext(DbContextOptions<MetriflowDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MetriflowDbContext).Assembly);
    }
}