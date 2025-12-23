using Metriflow.Correlation.Worker.Interfaces;
using Metriflow.DTOs;
using StackExchange.Redis;

namespace Metriflow.Correlation.Worker;

public class RecordsMatcher : IRecordsMatcher
{
    private readonly ILogger<RecordsMatcher> _logger;
    private readonly IRedisQueriesCorrelation _redisQueriesCorrelation;
    private readonly ICombiner _combiner;
    private readonly IRowDataProducer _producer;
    private const int HOURS_PER_DAY = 24;

    public RecordsMatcher(
        ILogger<RecordsMatcher> logger,
        IRedisQueriesCorrelation redis,
        ICombiner combiner,
        IRowDataProducer producer
    )
    {
        _logger = logger;
        _redisQueriesCorrelation = redis;
        _combiner = combiner;
        _producer = producer;
    }

    public async Task MatchRecords(
        Dictionary<enRedisCompletedListsNames, IEnumerable<string>> listsKeys,
        string[] listsPrefixes
    )
    {
        if (listsKeys == null || !listsKeys.Any())
            throw new ArgumentException("Lists keys cannot be empty", nameof(listsKeys));

        var keysSet = new HashSet<string>(listsKeys.First().Value);

        foreach (var (completedListTypeName, keys) in listsKeys)
        {
            foreach (var key in keys)
            {
                if (!keysSet.Contains(key))
                    continue;

                var ids = listsPrefixes.Select(prefix => $"{prefix}|{key}");
                var matchedRecords = await TryProcessRecordPairAsync(ids);

                if (
                    matchedRecords != null
                    && matchedRecords.Any()
                    && matchedRecords.All(r => r != null)
                )
                {
                    await _producer.PublishRawRecord(matchedRecords);
                    keysSet.Remove(key);
                    await _redisQueriesCorrelation.RemoveKeysFromCompletedLists(ids);
                }
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="sharedKey">The key without the prefix.</param>
    /// <param name="listsPrefixes">All lists prefixes</param>
    /// <remarks>Concat the shared key with each prefix, then get all data inside each list.</remarks
    /// <returns>List of records</returns>
    private async Task<IEnumerable<CombinedAnalyticsMessage>> TryProcessRecordPairAsync(
        IEnumerable<string> ids
    )
    {
        var redisValueDict = await _redisQueriesCorrelation.ExecutePopTransactionAsync(ids);
        var deserializedObjects = Helpers.RecordsDeserialization(redisValueDict, _logger);

        if (!AnalyticRecordsCombiner.CanCombine(deserializedObjects))
        {
            _logger.LogWarning("Records cannot be combined due to missing types.");
            return null;
        }
        return AnalyticRecordsCombiner.Combine(deserializedObjects);
    }
}
