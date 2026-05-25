using Metriflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure.EntitiesConfigurations;

public class MonthlyAnalyticsConfiguration : IEntityTypeConfiguration<MonthlyAnalytic>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<MonthlyAnalytic> builder
    )
    {
       
        builder.HasKey(ds => new { ds.PageId, ds.YearMonth }).IsClustered(false);
    }
}