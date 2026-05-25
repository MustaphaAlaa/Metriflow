using Metriflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure.EntitiesConfigurations;

public class TimeIntervalAnalyticsConfiguration : IEntityTypeConfiguration<TimeIntervalAnalytic>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TimeIntervalAnalytic> builder
    )
    {
        builder.HasNoKey();
    }
}