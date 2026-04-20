using Metriflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure.EntitiesConfigurations;

public class TimeIntervalAnalyticsConfiguration : IEntityTypeConfiguration<TimeIntervalAnalytic>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TimeIntervalAnalytic> builder
    )
    {
        // builder
        //     .HasAlternateKey(ds => (new { ds.PageId, ds.YearMonth }))
        //     .HasName("AK_MonthlyStat_PageMonth");

        builder.HasKey(ds => new { ds.PageId, ds.Date, ds.TimeIntervalId });
    }
}