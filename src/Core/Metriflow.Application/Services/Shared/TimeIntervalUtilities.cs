using System.ComponentModel;
using System.Reflection;
using Metriflow.Domain.Entities.Enums;

namespace Metriflow.Application.Services;

public static class TimeIntervalUtilities
{
    public static IReadOnlyDictionary<enTimeIntervals, string> Dictionary = Enum.GetValues<enTimeIntervals>()
        .ToDictionary(timeIntervals => timeIntervals,
            v => v!.GetType()
                .GetCustomAttribute<DescriptionAttribute>()!
                .Description ?? v.ToString());
    
    public static enTimeIntervals GetTimeInterval(int hour)
    {
         
        var interval = hour switch
        {
            < 4 => enTimeIntervals.First,
            < 8 => enTimeIntervals.Second,
            < 12 => enTimeIntervals.Third,
            < 16 => enTimeIntervals.Fourth,
            < 20 => enTimeIntervals.Fifth,
            _ => enTimeIntervals.Sixth,
        };
        return interval;
    }
}