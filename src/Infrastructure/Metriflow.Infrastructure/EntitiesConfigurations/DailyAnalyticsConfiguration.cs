using Metriflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure.EntitiesConfigurations;

public class DailyAnalyticsConfiguration : IEntityTypeConfiguration<DailyAnalytics>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<DailyAnalytics> builder
    )
    {
        
        builder.HasKey(ds => new { ds.PageId, ds.Date });
    }
}