using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Caches;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Metriflow.Application.Services.Workers;

[ServiceRegistration(ServiceLifetime.Scoped, typeof(IRecordMatchingWorkflow))]
public class RecordMatchingWorkflow(
    IAnalyticsCacheServices cache,
    IAnalyticRecordsDeserializer deserializer,
    IAnalyticRecordsCombiner combiner)
    : IRecordMatchingWorkflow
{
    public async Task<IList<CombinedAnalyticsMessage>?> TryMatchAsync(List<string> keys)
    {
        var removed = await cache.ExecutePopTransactionAsync(keys);
        var records = deserializer.Deserialize(removed);

        if (!combiner.CanCombine(records))
            return null;

        return combiner.Combine(records);
    }
}
