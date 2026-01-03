using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Caches;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Metriflow.Application.Services.Workers;

[ServiceRegistration(ServiceLifetime.Scoped, typeof(IRecordMatchingWorkflow))]
public class RecordMatchingWorkflow : IRecordMatchingWorkflow
{
    private readonly IAnalyticsCacheServices _cache;
    private readonly IAnalyticRecordsDeserializer _deserializer;
    private readonly IAnalyticRecordsCombiner _combiner;

    public RecordMatchingWorkflow(
        IAnalyticsCacheServices cache,
        IAnalyticRecordsDeserializer deserializer,
        IAnalyticRecordsCombiner combiner
    )
    {
        _cache = cache;
        _deserializer = deserializer;
        _combiner = combiner;
    }

    public async Task<IList<CombinedAnalyticsMessage>?> TryMatchAsync(List<string> keys)
    {
        var removed = await _cache.ExecutePopTransactionAsync(keys);
        var records = _deserializer.Deserialize(removed);

        if (!_combiner.CanCombine(records))
            return null;

        return _combiner.Combine(records);
    }
}
