using Metriflow.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure;



public partial class MetriflowDbContext : IdentityDbContext
{
    public MetriflowDbContext(DbContextOptions<MetriflowDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MetriflowDbContext).Assembly);
    }
}
