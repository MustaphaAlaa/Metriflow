using System.Collections;
using System.Text.Json;
using Metriflow.Application.Interfaces;
using Metriflow.Application.Services.Workers;
using Metriflow.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Metriflow.Redis;


public   class RedisAnalyticRecordDeserializer : IAnalyticRecordsDeserializer
{
    private readonly ILogger<RedisAnalyticRecordDeserializer> _logger;
    private readonly IReadOnlyDictionary<string,Type> _analyticReadTypesDictionary = AnalyticRecordTypeResolver.ResolveByKey();

    public RedisAnalyticRecordDeserializer(
        ILogger<RedisAnalyticRecordDeserializer> logger)
    {
        _logger = logger;
    }

    public Dictionary<Type, IList> Deserialize(
        Dictionary<string, IEnumerable<byte[]>> redisData)
    {
        var result = _analyticReadTypesDictionary.Values.ToDictionary(
            t => t,
            t => (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(t))!
        );

        foreach (var (listKey, recordsBytes) in redisData)
        {
            var prefix = listKey.Split('|')[0];

            if (!_analyticReadTypesDictionary.TryGetValue(prefix, out var recordType))
                continue;

            var list = result[recordType];

            foreach (var value in recordsBytes)
            {
                if (value?.Length ==0)
                {
                    _logger.LogWarning("Null Redis value for key {Key}", listKey);
                    continue;
                }

                try
                {
                    var record = (IAnalyticRecord)
                        JsonSerializer.Deserialize(value!, recordType)!;

                    list.Add(record);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize record for key {Key}", listKey);
                }
            }
        }

        return result;
    }
}