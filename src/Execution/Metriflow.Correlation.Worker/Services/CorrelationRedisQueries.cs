using Metriflow.Correlation.Worker.Interfaces;
using StackExchange.Redis;

namespace Metriflow.Correlation.Worker;

public class RedisQueriesCorrelation : IRedisQueriesCorrelation
{
    private readonly ILogger<RedisQueriesCorrelation> _logger;
    private readonly IDatabase _redis;

    const byte HOUR_PER_DAY = 24;

    public RedisQueriesCorrelation(ILogger<RedisQueriesCorrelation> logger, IDatabase redis)
    {
        _logger = logger;
        _redis = redis;
    }

    // public async Task<(RedisValue[] GAKeys, RedisValue[] PSIKeys)> GetListsKeys()
    // {
    //     var listKeysBatch = _redis.CreateBatch();

    //     var listGAKeys = listKeysBatch.ListRangeAsync(
    //         enRedisListsNames.CompletedListGA.ToString(),
    //         0,
    //         -1
    //     );

    //     var listPSIKeys = listKeysBatch.ListRangeAsync(
    //         enRedisListsNames.CompletedListPSI.ToString(),
    //         0,
    //         -1
    //     );

    //     listKeysBatch.Execute();

    //     await Task.WhenAll(listGAKeys, listPSIKeys);

    //     return (listGAKeys.Result, listPSIKeys.Result);
    // }

    // public (Task<long>, Task<long>) RemoveKeysFromCompletedLists(
    //     ITransaction transaction,
    //     string gaKey,
    //     string psiKey
    // )
    // {
    //     var removeGAFromList = transaction.ListRemoveAsync(
    //         enRedisListsNames.CompletedListGA.ToString(),
    //         gaKey
    //     );

    //     var removePSIFromList = transaction.ListRemoveAsync(
    //         enRedisListsNames.CompletedListPSI.ToString(),
    //         psiKey
    //     );
    //     return (removeGAFromList, removePSIFromList);
    // }

    // public void PopAndAddListsItemsTo(
    //     ITransaction transaction,
    //     List<Task<RedisValue>> redisValueGA,
    //     string gaKey,
    //     List<Task<RedisValue>> redisValuePSI,
    //     string psiKey
    // )
    // {
    //     for (byte i = 0; i < HOUR_PER_DAY; i++)
    //     {
    //         redisValueGA.Add(transaction.ListLeftPopAsync(gaKey));
    //         redisValuePSI.Add(transaction.ListLeftPopAsync(psiKey));
    //     }
    // }
}
