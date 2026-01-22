using System.ComponentModel;
using System.Reflection;
using Metriflow.Domain.Entities.Enums;

namespace Metriflow.Application.Services;

public static class TimeIntervalUtilities
{
    // public static IReadOnlyDictionary<enTimeIntervals, string> Dictionary = Enum.GetValues<enTimeIntervals>()
    //     .ToDictionary(timeIntervals => timeIntervals,
    //         v => v!.GetType()
    //             .GetCustomAttribute<DescriptionAttribute>()!
    //             .Description ?? v.ToString());


    public static readonly IReadOnlyDictionary<enTimeIntervals, string> Descriptions =
        new Dictionary<enTimeIntervals, string>
        {
            [enTimeIntervals.First] = "12-hour: 12:00 AM – 3:59 AM | 24-hour: 00:00 – 03:59",
            [enTimeIntervals.Second] = "12-hour: 4:00 AM – 7:59 AM | 24-hour: 04:00 – 07:59",
            [enTimeIntervals.Third] = "12-hour: 8:00 AM – 11:59 AM | 24-hour: 08:00 – 11:59",
            [enTimeIntervals.Fourth] = "12-hour: 12:00 PM – 3:59 PM | 24-hour: 12:00 – 15:59",
            [enTimeIntervals.Fifth] = "12-hour: 4:00 PM – 7:59 PM | 24-hour: 16:00 – 19:59",
            [enTimeIntervals.Sixth] = "12-hour: 8:00 PM – 11:59 PM | 24-hour: 20:00 – 23:59"
        };


    public static int GetTimeInterval(int hour)
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
        return (int)interval;
    }
}