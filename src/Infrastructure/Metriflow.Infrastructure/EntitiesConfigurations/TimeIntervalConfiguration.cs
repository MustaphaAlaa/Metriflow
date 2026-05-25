
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
        try
        {
            builder.HasIndex(timeInterval => timeInterval.Interval).IsUnique();
            builder.Property(timeInterval => timeInterval.Interval).IsRequired();
            builder.HasData(this.TimeIntervalList());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception In TimIntervalConfiguration");
            Console.WriteLine(ex.Message);
            Console.WriteLine();
        }
    }

    private List<TimeInterval> TimeIntervalList()
    {
        try
        {
            var dic = TimeIntervalUtilities.Descriptions;
            return dic.Select(keyValuePair => new TimeInterval()
                {
                    Id = (int)keyValuePair.Key,
                    Interval = keyValuePair.Key,
                    IntervalDescription = keyValuePair.Value
                })
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }


        return null;
    }
}