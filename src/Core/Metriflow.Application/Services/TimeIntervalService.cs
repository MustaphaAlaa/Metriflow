using System.ComponentModel;
using System.Reflection;
using Metriflow.Domain.Entities.Enums;

namespace Metriflow.Application.Services;

public static class TimeIntervalService
{
    public static IReadOnlyDictionary<enTimeIntervals, string> Dictionary = Enum.GetValues<enTimeIntervals>()
        .ToDictionary(timeIntervals => timeIntervals,
            v => v!.GetType()
                .GetCustomAttribute<DescriptionAttribute>()!
                .Description ?? v.ToString());
}