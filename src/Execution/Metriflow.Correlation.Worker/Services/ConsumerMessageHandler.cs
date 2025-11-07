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
    private const string MaxDateKey = "analytics:max_date";

    readonly Object locker = new();

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
    public async Task HandleIncomingRecordAsync<T>(string type, T record)
        where T : IAnalyticRecord
    {
        _logger.LogInformation($"Start Handling incoming Request, {type} ==> {record}");

        DateOnly date = record.Date;
        string page = record.Page;

        string fieldKey = $"{date}|{page}";

        await _redis.HashSetAsync(
            key: type,
            hashFields: new HashEntry[]
            {
                new HashEntry(
                    fieldKey,
                    JsonSerializer.SerializeToUtf8Bytes(record, JsonSetting.SerializerOptions)
                ),
            }
        );

        _logger.LogDebug($"Saved record for type '{type}' with key '{fieldKey}'.");

        lock (locker)
        {
            RedisValue maxDateValue = _redis.StringGetAsync(MaxDateKey).GetAwaiter().GetResult();

            DateOnly currentMaxDate =
                maxDateValue.HasValue
                && DateOnly.TryParse(maxDateValue.ToString(), out DateOnly parsedDate)
                    ? parsedDate
                    : DateOnly.MinValue;

            _logger.LogWarning($"Current Max data in the redis => {currentMaxDate}");
            if (date > currentMaxDate)
            {
                var originalMaxDateValue = _redis
                    .StringGetSetAsync(MaxDateKey, date.ToString())
                    .GetAwaiter()
                    .GetResult();

                _logger.LogWarning(
                    $"Original Max Date {originalMaxDateValue} --------- New Max Date {MaxDateKey}"
                );

                if (originalMaxDateValue.HasValue)
                {
                    if (DateOnly.TryParse(originalMaxDateValue.ToString(), out DateOnly oldMaxDate))
                    {
                        if (date > oldMaxDate)
                        {
                            _logger.LogInformation(
                                $"Successfully updated MaxDate to {date}. Triggering combine for OLD max date: {oldMaxDate}."
                            );

                            this.MatchRecords(date).GetAwaiter().GetResult();
                        }
                        else
                        {
                            _logger.LogDebug(
                                $"Race condition: Max date already updated by another process to {date}. Skipping MatchDay."
                            );
                        }
                    }
                }
                else
                {
                    _logger.LogInformation(
                        $"Initialized MaxDate to {date}. No previous date to combine."
                    );
                }
            }
            else
            {
                _logger.LogDebug(
                    $"Incoming date {date} is not beyond current max date {currentMaxDate}. No combine triggered."
                );
            }
        }
    }

    /// <summary>
    /// Match previously-stored PSI and GA records for the given date, and trigger combining,
    ///  then trigger DeleteFields method to delete them from redis after the publishing.
    /// </summary>
    private async Task MatchRecords(DateOnly date)
    {
        var pp = await _redis.HashGetAllAsync("psi");
        var gg = await _redis.HashGetAllAsync("ga");

        if (pp.Length != gg.Length)
        {
            _logger.LogInformation(
                "Cannot match and delete psi and ga the records not same length"
            );
        }

        var psiFields = pp.Select(p => p.Name);
        var gaFields = gg.Select(g => g.Name);

        var psiKeys = psiFields
            .Where(psi => !psi.StartsWith(date.ToString()))
            .Select(f => f.ToString().Replace("psi:", ""))
            .ToHashSet();

        var gaKeys = gaFields
            .Where(ga => !ga.StartsWith(date.ToString()))
            .Select(f => f.ToString().Replace("ga:", ""))
            .ToHashSet();

        var commonKeys = psiKeys.Intersect(gaKeys);
        var GA_PSI_List = await Get_GA_PSI_LIST(commonKeys);
        ;

        if (GA_PSI_List.Count > 0)
            await _combiner.GA_PSI_Combiner(GA_PSI_List);

        if (commonKeys.Count() > 0)
            await DeleteFields(commonKeys);
    }

    /// <summary>
    /// Retrieve combined GA/PSI records for the provided keys from Redis and deserialize them.
    /// </summary>
    private async Task<List<Tuple<GARecord, PSIRecord>>> Get_GA_PSI_LIST(IEnumerable<string> keys)
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
