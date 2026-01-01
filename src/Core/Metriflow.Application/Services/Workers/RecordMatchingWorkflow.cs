using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Caches;
using Metriflow.Domain.Entities;

namespace Metriflow.Application.Services.Workers;

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
