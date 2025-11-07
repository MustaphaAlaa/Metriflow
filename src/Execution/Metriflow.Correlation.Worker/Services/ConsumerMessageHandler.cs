using System.Text;
using System.Text.Json;
using Metriflow.Correlation.Worker.Interfaces;
using StackExchange.Redis;

namespace Metriflow.Correlation.Worker;

public class ConsumerMessageHandler : IConsumerMessageHandler
{
    private readonly ILogger<CorrelationWorker> _logger;
    private readonly IDatabase _redis;
    private readonly ICombiner _combiner;
    private const string MaxDateKey = "analytics:max_date";

    readonly Object locker = new();

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

        foreach (var key in commonKeys)
        {
            var gaBytes = await _redis.HashGetAsync("ga", key);
            var psiBytes = await _redis.HashGetAsync("psi", key);

            var ga = JsonSerializer.Deserialize<GARecord>(gaBytes, JsonSetting.SerializerOptions);

            var psi = JsonSerializer.Deserialize<PSIRecord>(
                psiBytes,
                JsonSetting.SerializerOptions
            );

            // Combine data and produce it
            await _combiner.GA_PSI_Combiner(ga, psi);

            var gaDeleted = await _redis.HashDeleteAsync("ga", key);
            var psiDeleted = await _redis.HashDeleteAsync("psi", key);
        }
    }
}
