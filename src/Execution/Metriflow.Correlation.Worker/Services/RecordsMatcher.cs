using System.Text.Json;
using Metriflow.Correlation.Worker.Interfaces;
using StackExchange.Redis;

namespace Metriflow.Correlation.Worker;

public class RecordsMatcher : IRecordsMatcher
{
    private readonly ILogger<RecordsMatcher> _logger;
    private readonly IDatabase _redis;
    private readonly ICombiner _combiner;

    public RecordsMatcher(ILogger<RecordsMatcher> logger, IDatabase redis, ICombiner combiner)
    {
        _logger = logger;
        _redis = redis;
        _combiner = combiner;
    }

    public async Task MatchRecords()
    {
        RedisValue[] listGA = await _redis.ListRangeAsync(
            enRedisListsNames.CompletedListGA.ToString(),
            0,
            -1
        );
        RedisValue[] listPSI = await _redis.ListRangeAsync(
            enRedisListsNames.CompletedListPSI.ToString(),
            0,
            -1
        );
        foreach (var rec in listGA) { }

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
