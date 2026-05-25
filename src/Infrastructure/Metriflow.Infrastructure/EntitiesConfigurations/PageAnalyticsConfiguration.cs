using Metriflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure.EntitiesConfigurations;

public class PageAnalyticsConfiguration : IEntityTypeConfiguration<PageAnalytics>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<PageAnalytics> builder
    )
    {
        builder.HasNoKey();

        builder
            .HasIndex(pa => new
            {
                pa.PageId,
                pa.DateOnly,
                Intervals = pa.Interval,
            })
            .HasDatabaseName("IX_PageAnalytics_ReAggregation")
            .IsClustered(false);

        builder
            .HasIndex(pa => pa.CreatedAt)
            .HasDatabaseName("IX_PageAnalytics_CreatedAt")
            .IsClustered(false);
    }
}
