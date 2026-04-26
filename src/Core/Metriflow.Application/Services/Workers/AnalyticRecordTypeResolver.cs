using System.Reflection;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Interfaces;

namespace Metriflow.Application.Services.Workers;

public static class AnalyticRecordTypeResolver
{
    public static IReadOnlyDictionary<string, Type> ResolveByKey()
    {
        return typeof(IAnalyticRecord)
            .Assembly
            .GetTypes()
            .Where(t =>
                typeof(IAnalyticRecord).IsAssignableFrom(t) &&
                t.GetCustomAttribute<AnalyticRecordAttribute>()?.Key != null
            )
            .ToDictionary(
                t => t.GetCustomAttribute<AnalyticRecordAttribute>()!.Key,
                t => t
            );
    }
}

