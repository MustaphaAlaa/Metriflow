using StackExchange.Redis;

namespace Metriflow.Correlation.Worker.Interfaces;

public interface IRedisQueriesCorrelation
{
    Task<(RedisValue[] GAKeys, RedisValue[] PSIKeys)> GetListsKeys();
    void PopAndAddListsItemsTo(
        ITransaction transaction,
        List<Task<RedisValue>> redisValueGA,
        string gaKey,
        List<Task<RedisValue>> redisValuePSI,
        string psiKey
    );
    (Task<long>, Task<long>) RemoveKeysFromCompletedLists(
        ITransaction transaction,
        string gaKey,
        string psiKey
    );
}
