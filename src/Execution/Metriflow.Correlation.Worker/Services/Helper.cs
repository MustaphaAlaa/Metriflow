using System.Text.Json;
using Metriflow.Correlation.Worker.Interfaces;
using Metriflow.Domain;
using Metriflow.Messaging.interfaces;
using StackExchange.Redis;

namespace Metriflow.Correlation.Worker;

/// <summary>
/// Utility helper used by the Correlation worker to scan Redis for matching GA and PSI records,
/// combine them, and remove matched entries from Redis.
/// </summary>
/// <remarks>
/// - Reads all fields from the "psi" and "ga" hashes, extracts keys by removing the prefix ("psi:" / "ga:"),
///   finds the intersection of keys present in both, deserializes matching entries and calls an internal
///   combiner method for further processing.
/// - Deletes matched entries from the Redis hashes after processing.
/// - This implementation loads all keys into memory — it may not scale for very large hash sizes.
/// </remarks>
public class Helper : IHelper
{
    private readonly ILogger<Helper> _logger;
    private readonly IDatabase _redis;
    private readonly ICombiner _Combiner;

    public Helper(ILogger<Helper> logger, ICombiner combiner, IConnectionMultiplexer redis)
    {
        _Combiner = combiner;
        _logger = logger;
        _redis = redis.GetDatabase();
    }

    public async Task MatchAll()
    {
        var psiFields = await _redis.HashKeysAsync("psi");
        var gaFields = await _redis.HashKeysAsync("ga");

        var psiKeys = psiFields.Select(f => f.ToString().Replace("psi:", "")).ToHashSet();
        var gaKeys = gaFields.Select(f => f.ToString().Replace("ga:", "")).ToHashSet();

        var commonKeys = psiKeys.Intersect(gaKeys);

        foreach (var key in commonKeys)
        {
            var gaBytes = await _redis.HashGetAsync("ga", "ga:" + key);
            var psiBytes = await _redis.HashGetAsync("psi", "psi:" + key);

            var ga = JsonSerializer.Deserialize<GARecord>(gaBytes, JsonSetting.SerializerOptions);
            var psi = JsonSerializer.Deserialize<PSIRecord>(
                psiBytes,
                JsonSetting.SerializerOptions
            );

            // Combine data and produce it
            await _Combiner.GA_PSI_Combiner(ga, psi);

            await _redis.HashDeleteAsync("ga", "ga:" + key);
            await _redis.HashDeleteAsync("psi", "psi:" + key);
        }
    }
}
