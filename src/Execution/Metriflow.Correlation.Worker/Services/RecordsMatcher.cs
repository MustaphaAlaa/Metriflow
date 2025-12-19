using System.Diagnostics;
using System.Text.Json;
using Metriflow.Correlation.Worker.Interfaces;
using StackExchange.Redis;

namespace Metriflow.Correlation.Worker;

public class RecordsMatcher : IRecordsMatcher
{
    private readonly ILogger<RecordsMatcher> _logger;
    private readonly IDatabase _redis;
    private readonly ICombiner _combiner;

    private const int HOURS_PER_DAY = 24;
    private const string GA_PREFIX = "ga|";
    private const string PSI_PREFIX = "psi|";

    public RecordsMatcher(ILogger<RecordsMatcher> logger, IDatabase redis, ICombiner combiner)
    {
        _logger = logger;
        _redis = redis;
        _combiner = combiner;
    }

    public async Task MatchRecords()
    {
        var (listGAResult, listPSIResult) = await GetListsKeys();

        var gaKeysSet = new HashSet<string>(listGAResult.Select(ga => ExtractId(ga, GA_PREFIX)));

        var recordsList = new List<recordGA_PSI>(HOURS_PER_DAY);
        var redisValueGA = new List<Task<RedisValue>>(HOURS_PER_DAY);
        var redisValuePSI = new List<Task<RedisValue>>(HOURS_PER_DAY);

        foreach (var rec in listPSIResult)
        {
            var id = ExtractId(rec, PSI_PREFIX);

            if (!gaKeysSet.Contains(id))
                continue;

            var matchedRecords = await TryProcessRecordPairAsync(
                id,
                recordsList,
                redisValueGA,
                redisValuePSI
            );

            if (matchedRecords != null || matchedRecords.Count > 0)
            {
                await _combiner.GA_PSI_Combiner(recordsList);

                gaKeysSet.Remove(id);
            }
        }
    }

    private async Task<List<recordGA_PSI>> TryProcessRecordPairAsync(
        string id,
        List<recordGA_PSI> recordsList,
        List<Task<RedisValue>> redisValueGA,
        List<Task<RedisValue>> redisValuePSI
    )
    {
        recordsList.Clear();
        redisValueGA.Clear();
        redisValuePSI.Clear();

        var gaKey = $"{GA_PREFIX}{id}";
        var psiKey = $"{PSI_PREFIX}{id}";

        bool commit = await ExecutePopTransactionAsync(redisValueGA, redisValuePSI, gaKey, psiKey);

        if (!commit)
        {
            _logger.LogWarning(
                $"Transaction failed for ID {id} - lists may not have exactly {HOURS_PER_DAY} items"
            );
            return null;
        }

        if (redisValueGA.Count != redisValuePSI.Count)
            return null;

        var (lstGA, lstPSI) = await this.RecordsDeserialization(id, redisValueGA, redisValuePSI);

        if (lstGA.Count != lstPSI.Count || lstGA.Count == 0)
        {
            _logger.LogWarning(
                $"Record count mismatch or empty for ID {id}. GA: {lstGA.Count}, PSI: {lstPSI.Count}"
            );
            return null;
        }
        CombineMatchingRecords(recordsList, lstGA, lstPSI);
        return recordsList.Count > 0 ? recordsList : null;
    }

    private static void CombineMatchingRecords(
        List<recordGA_PSI> outputList,
        List<GARecord> lstGA,
        List<PSIRecord> lstPSI
    )
    {
        var GaDict = lstGA.ToDictionary(ga => ga!.Date, ga => ga)!;

        foreach (var item in lstPSI)
        {
            if (!GaDict.ContainsKey(item.Date))
                continue;

            outputList.Add(new recordGA_PSI(GaDict[item.Date], item));
        }
    }

    private async Task<bool> ExecutePopTransactionAsync(
        List<Task<RedisValue>> redisValueGA,
        List<Task<RedisValue>> redisValuePSI,
        string gaKey,
        string psiKey
    )
    {
        var transaction = _redis.CreateTransaction();

        transaction.AddCondition(Condition.ListLengthEqual(gaKey, HOURS_PER_DAY));
        transaction.AddCondition(Condition.ListLengthEqual(psiKey, HOURS_PER_DAY));

        PopAndAddListsItemsTo(transaction, redisValueGA, gaKey, redisValuePSI, psiKey);

        RemoveKeysFromCompletedLists(transaction, gaKey, psiKey);

        var commit = await transaction.ExecuteAsync();
        return commit;
    }

    private string ExtractId(RedisValue key, string prefix)
    {
        var s = key.ToString();
        return s.StartsWith(prefix) ? s[prefix.Length..] : s;
    }

    private async Task<(List<GARecord>, List<PSIRecord>)> RecordsDeserialization(
        string id,
        List<Task<RedisValue>> redisValueGA,
        List<Task<RedisValue>> redisValuePSI
    )
    {
        var lstGA = new List<GARecord>();
        var lstPSI = new List<PSIRecord>();

        await Task.WhenAll(redisValueGA.Concat(redisValuePSI));

        for (int i = 0; i < HOURS_PER_DAY; i++)
        {
            try
            {
                var gaValue = redisValueGA[i].Result;
                var psiValue = redisValuePSI[i].Result;

                if (psiValue.IsNullOrEmpty || gaValue.IsNullOrEmpty)
                {
                    _logger.LogError($"Null value found for ID {id} at hour {i}");
                    continue;
                }

                var gaRecord = JsonSerializer.Deserialize<GARecord>(gaValue!);
                var psiRecord = JsonSerializer.Deserialize<PSIRecord>(psiValue!);

                if (gaRecord == null || psiRecord == null)
                {
                    _logger.LogError($"Deserialization failed for ID {id} at hour {i}");
                    continue;
                }

                lstGA.Add(gaRecord);
                lstPSI.Add(psiRecord);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"JSON deserialization error for ID {id} at hour {i}");
            }
        }
        return (lstGA, lstPSI);
    }

    private async Task<(RedisValue[] GAKeys, RedisValue[] PSIKeys)> GetListsKeys()
    {
        var listKeysBatch = _redis.CreateBatch();

        var listGAKeys = listKeysBatch.ListRangeAsync(
            enRedisListsNames.CompletedListGA.ToString(),
            0,
            -1
        );

        var listPSIKeys = listKeysBatch.ListRangeAsync(
            enRedisListsNames.CompletedListPSI.ToString(),
            0,
            -1
        );

        listKeysBatch.Execute();

        await Task.WhenAll(listGAKeys, listPSIKeys);

        return (listGAKeys.Result, listPSIKeys.Result);
    }

    private (Task<long>, Task<long>) RemoveKeysFromCompletedLists(
        ITransaction transaction,
        string gaKey,
        string psiKey
    )
    {
        var removeGAFromList = transaction.ListRemoveAsync(
            enRedisListsNames.CompletedListGA.ToString(),
            gaKey
        );

        var removePSIFromList = transaction.ListRemoveAsync(
            enRedisListsNames.CompletedListPSI.ToString(),
            psiKey
        );
        return (removeGAFromList, removePSIFromList);
    }

    private void PopAndAddListsItemsTo(
        ITransaction transaction,
        List<Task<RedisValue>> redisValueGA,
        string gaKey,
        List<Task<RedisValue>> redisValuePSI,
        string psiKey
    )
    {
        for (byte i = 0; i < HOURS_PER_DAY; i++)
        {
            redisValueGA.Add(transaction.ListLeftPopAsync(gaKey));
            redisValuePSI.Add(transaction.ListLeftPopAsync(psiKey));
        }
    }
}
