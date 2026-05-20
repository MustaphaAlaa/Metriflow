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
           .HasName("IX_PageAnalytics_ReAggregation")
           .IsClustered(false);

           
    }
}
