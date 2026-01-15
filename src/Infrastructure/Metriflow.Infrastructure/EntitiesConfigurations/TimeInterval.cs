using Metriflow.Application.Services;
using Metriflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure.EntitiesConfigurations;

public class TimeIntervalConfiguration : IEntityTypeConfiguration<TimeInterval>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TimeInterval> builder
    )
    {
        builder.HasIndex(timeInterval => timeInterval.Interval).IsUnique();
        builder.Property(timeInterval => timeInterval.Interval).IsRequired();
        builder.HasData(this.TimeIntervalList());
    }

    private List<TimeInterval> TimeIntervalList()
    {
        return TimeIntervalUtilities.Dictionary.Select(keyValuePair => new TimeInterval()
            {
                Interval = keyValuePair.Key,
                IntervalDescription = keyValuePair.Value
            })
            .ToList();
    }
}