using Metriflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure.EntitiesConfigurations;

public class PageAnalyticsConfiguration : IEntityTypeConfiguration<PageAnalytics>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<PageAnalytics> builder
    )
    {
        builder.HasIndex(pa => pa.PageId);
        builder.HasIndex(pa => new { pa.PageId, pa.Date }).IsUnique();
    }
}