using Metriflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure.EntitiesConfigurations;

public class DailyStatConfiguration : IEntityTypeConfiguration<DailyAnalytics>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<DailyAnalytics> builder
    )
    { 
        builder.HasIndex(ds => ds.Date);
    }
}
