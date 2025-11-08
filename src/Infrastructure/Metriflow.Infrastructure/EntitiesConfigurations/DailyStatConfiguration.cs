using Metriflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure.EntitiesConfigurations;

public class DailyStatConfiguration : IEntityTypeConfiguration<DailyStat>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<DailyStat> builder
    )
    {
        builder
            .HasAlternateKey(ds => (new { ds.PageId, ds.Date }))
            .HasName("AK_DailyStats_PageDay");

        builder.HasIndex(ds => ds.Date);
    }
}
