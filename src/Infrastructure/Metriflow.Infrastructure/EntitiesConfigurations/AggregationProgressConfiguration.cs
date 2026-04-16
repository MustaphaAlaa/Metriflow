using Metriflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure.EntitiesConfigurations;

public class AggregationProgressConfiguration : IEntityTypeConfiguration<AggregationProgress>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<AggregationProgress> builder
    )
    {
        builder.HasKey(e => new { e.PageId, e.Date })
            .HasName("PK_AggregationProgress");
        builder.HasIndex(e => new { e.Correlation });
        builder.HasIndex(e => new { e.Interval });
        builder.HasIndex(e => new { e.Daily });
        builder.HasIndex(e => new { e.Monthly });
        builder.HasIndex(e => new { e.Quarterly });
        builder.HasIndex(e => new { e.Yearly });
    }
}
