using Metriflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure.EntitiesConfigurations;

public class MonthlyAnalyticsConfiguration : IEntityTypeConfiguration<MonthlyAnalytic>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<MonthlyAnalytic> builder
    )
    {
        builder
            .HasAlternateKey(ds => (new { ds.PageId, ds.YearMonth }))
            .HasName("AK_MonthlyStat_PageMonth");

        builder.HasIndex(ds => ds.YearMonth);
    }
}