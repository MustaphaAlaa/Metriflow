using Metriflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure.EntitiesConfigurations;

public class PageAnalyticsConfiguration : IEntityTypeConfiguration<PageAnalytics>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<PageAnalytics> builder
    )
    {
        builder.HasKey(pa => pa.Id);
        builder.HasIndex(pa => new { pa.PageId, pa.Date }).IsUnique();
    }
}