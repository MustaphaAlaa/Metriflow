using StackExchange.Redis;

namespace Metriflow.Correlation.Worker.Interfaces;

public interface IRedisQueriesCorrelation
{
    Task<Dictionary<string, IEnumerable<RedisValue>>> ExecutePopTransactionAsync(
        IEnumerable<string> listsKeys,
        int expectedListLength = 24
    );
    Task<Dictionary<enRedisCompletedListsNames, IEnumerable<string>>> GetCompletedListsKeys();
    Task<bool> RemoveKeysFromCompletedLists(IEnumerable<string> listsKeys);
}
