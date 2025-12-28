using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Caches;
using Metriflow.Application.Interfaces.Workers;
using Metriflow.Application.Models.Enums;
using Metriflow.Application.Worker;
using Metriflow.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Metriflow.Correlation.Worker;

public class RecordsMatcher : IRecordsMatcher
{
    private readonly ILogger<RecordsMatcher> _logger;
    private readonly IAnalyticsCacheServices _analyticsCacheServices;
    private const string ExchangeName = "analytics.raw";
    private const int HoursPerDay = 24;
    private readonly IAnalyticRecordsDeserializer _analyticRecordsDeserializer;
    private readonly IMessageBrokerProducer _messageBrokerProducer;
    
    // private readonly IRowDataProducer _producer;
    // private readonly IEnumerable<Type> _analyticRecordTypes = Helpers.GetAllAnalyticRecordTypes();


    public RecordsMatcher(
        ILogger<RecordsMatcher> logger,
        IAnalyticsCacheServices analyticsCacheServices,
        IAnalyticRecordsDeserializer analyticRecordsDeserializer,
        IMessageBrokerProducer messageBrokerProducer
    )
    {
        _logger = logger;
        _analyticsCacheServices = analyticsCacheServices;
        _analyticRecordsDeserializer = analyticRecordsDeserializer;
        _messageBrokerProducer = messageBrokerProducer;
    }

    public async Task MatchRecords(
        Dictionary<enCompletedListsNames, IEnumerable<string>> listsKeys,
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
                var combinedRecords = await TryProcessRecordPairAsync(ids);

                if (
                    combinedRecords != null
                    && combinedRecords.Any()
                    && combinedRecords.Any(r => r is not null)
                )
                {
                    await _messageBrokerProducer.PublishAsync(combinedRecords, ExchangeName, "analytics.raw", true);

                    await _analyticsCacheServices.RemoveKeysFromCompletedLists(ids);
                    keysSet.Remove(key);

                    _logger.LogInformation(
                        $"Published raw {combinedRecords.Count} records to '{ExchangeName}':\n{string.Join(" \t\t\n", combinedRecords)}");
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
    private async Task<IList<CombinedAnalyticsMessage>> TryProcessRecordPairAsync(
        IEnumerable<string> ids
    )
    {
        var redisValueDict = await _analyticsCacheServices.ExecutePopTransactionAsync(
            ids,
            HoursPerDay
        );
        //
        //
        //
        //
        //
        //
        // var deserializedObjects = Helpers.RecordsDeserialization(
        //     redisValueDict,
        //     _analyticRecordTypes,
        //     _logger
        // );

        var deserializedObjects = _analyticRecordsDeserializer.Deserialize(redisValueDict);


        if (!AnalyticRecordsCombiner.CanCombine(deserializedObjects))
        {
            _logger.LogWarning("Records cannot be combined due to missing types.");
            return null;
        }

        return AnalyticRecordsCombiner.Combine(deserializedObjects);
    }
}