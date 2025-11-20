using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Metriflow.Correlation.Worker.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Metriflow.Correlation.Worker;

/// <summary>
/// Default implementation of <see cref="IConsumerMessageHandler"/>. Stores incoming records in Redis,
/// tracks the maximum received date, and triggers matching/combining logic when appropriate.
/// </summary>
public class ConsumerMessageHandler : IConsumerMessageHandler
{
    private readonly ILogger<CorrelationWorker> _logger;
    private readonly IDatabase _redis;
    private readonly ICombiner _combiner;

    /// <summary>
    /// Creates a new <see cref="ConsumerMessageHandler"/>.
    /// </summary>
    public ConsumerMessageHandler(
        ILogger<CorrelationWorker> logger,
        IConnectionMultiplexer redis,
        ICombiner combiner
    )
    {
        _combiner = combiner;
        _logger = logger;
        _redis = redis.GetDatabase();
    }

    /// <inheritdoc />
    public async Task HandleIncomingRecordAsync<T>(string type, IList<T> record)
        where T : IAnalyticRecord
    {
        if (record is null || record.Count == 0)
        {
            _logger.LogDebug("HandleIncomingRecordAsync called with no items; nothing to handle.");
            return;
        }
        _logger.LogInformation(
            "Start Handling incoming Request, {type} ==> {record}",
            type,
            record
        );

        var date = await SaveNewRecordAsync(type, record);

        var maxDateUpdated = await TryUpdateMaxDate(date.ToString());

        if (maxDateUpdated)
        {
            _logger.LogInformation(
                $"Successfully updated MaxDate to {date}. Triggering combine for the previous day."
            );

            await this.MatchRecords(date);
        }
        else
        {
            _logger.LogDebug(
                "Incoming date {date} is not greater than current max date. No combine triggered.",
                date
            );
        }
    }

    private async Task<DateTime> SaveNewRecordAsync<T>(string type, IList<T> record)
        where T : IAnalyticRecord
    {
        DateTime date = default;
        foreach (var rec in record)
        {
            date = rec.Date;
            var page = rec.Page;

            var fieldKey = $"{date.ToString("yyyy-MM-dd")}|{page.ToString()}";
            await _redis.HashSetAsync(
                key: type,
                hashFields: new HashEntry[]
                {
                    new HashEntry(
                        fieldKey.ToString(),
                        JsonSerializer.SerializeToUtf8Bytes(rec, JsonSetting.SerializerOptions)
                    ),
                }
            );
            _logger.LogDebug(
                "Saved record for type '{type}' with key '{fieldKey}'.",
                type,
                fieldKey
            );
        }

        return date;
    }

    private async Task<bool> TryUpdateMaxDate(string date)
    {
        string lubaScript =
            @" local current = redis.call('GET', KEYS[1])
            if (not current) or (ARGV[1] > current) then
                redis.call('SET', KEYS[1], ARGV[1])
                return 1
            else
                return 0
            end
            ";

        var result = await _redis.ScriptEvaluateAsync(
            lubaScript,
            new RedisKey[] { "analytics:max_date" },
            new RedisValue[] { date }
        );

        return (int)result == 1;
    }

    /// <summary>
    /// Match previously-stored PSI and GA records for the given date, and trigger combining,
    ///  then trigger DeleteFields method to delete them from redis after the publishing.
    /// </summary>
    private async Task MatchRecords(DateTime date)
    {
        var psiRecords = await _redis.HashGetAllAsync("psi");
        var gaRecords = await _redis.HashGetAllAsync("ga");

        if (psiRecords.Length != gaRecords.Length)
        {
            _logger.LogInformation(
                "Cannot match and delete psi and ga the records not same length"
            );
            throw new Exception();
        }

        var psiFields = psiRecords.Select(p => p.Name);
        var gaFields = gaRecords.Select(g => g.Name);

        var psiKeys = psiFields
            .Where(psi => !psi.StartsWith(date.ToString()))
            .Select(f => f.ToString().Replace("psi:", ""))
            .ToHashSet();

        var gaKeys = gaFields
            .Where(ga => !ga.StartsWith(date.ToString()))
            .Select(f => f.ToString().Replace("ga:", ""))
            .ToHashSet();

        var commonKeys = psiKeys.Intersect(gaKeys);
        var GA_PSI_List = await GetGaPsiListAsync(commonKeys);
        ;

        if (GA_PSI_List.Count > 0)
            await _combiner.GA_PSI_Combiner(GA_PSI_List);

        if (commonKeys.Count() > 0)
            await DeleteFields(commonKeys);
    }

    /// <summary>
    /// Retrieve combined GA/PSI records for the provided keys from Redis and deserialize them.
    /// </summary>
    private async Task<List<Tuple<GARecord, PSIRecord>>> GetGaPsiListAsync(IEnumerable<string> keys)
    {
        var lst = new List<Tuple<GARecord, PSIRecord>>();

        foreach (var key in keys)
        {
            var gaBytes = await _redis.HashGetAsync("ga", key);
            var psiBytes = await _redis.HashGetAsync("psi", key);

            var ga = JsonSerializer.Deserialize<GARecord>(gaBytes, JsonSetting.SerializerOptions)!;

            var psi = JsonSerializer.Deserialize<PSIRecord>(
                psiBytes,
                JsonSetting.SerializerOptions
            )!;

            lst.Add(new Tuple<GARecord, PSIRecord>(ga, psi));
        }

        return lst;
    }

    /// <summary>
    /// Delete the GA and PSI fields matching the provided keys from Redis.
    /// </summary>
    private async Task DeleteFields(IEnumerable<string> keys)
    {
        foreach (var key in keys)
        {
            var gaDeleted = await _redis.HashDeleteAsync("ga", key);
            var psiDeleted = await _redis.HashDeleteAsync("psi", key);
        }
    }
}
