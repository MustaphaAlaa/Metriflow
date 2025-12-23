using Metriflow.Correlation.Worker.Interfaces;
using StackExchange.Redis;

namespace Metriflow.Correlation.Worker;

public class RedisQueriesCorrelation : IRedisQueriesCorrelation
{
    private readonly ILogger<RedisQueriesCorrelation> _logger;
    private readonly IDatabase _redis;

    public RedisQueriesCorrelation(ILogger<RedisQueriesCorrelation> logger, IDatabase redis)
    {
        _logger = logger;
        _redis = redis;
    }

    public async Task<
        Dictionary<enRedisCompletedListsNames, IEnumerable<string>>
    > GetCompletedListsKeys()
    {
        var listKeysBatch = _redis.CreateBatch();

        var pending = new Dictionary<enRedisCompletedListsNames, Task<RedisValue[]>>();

        foreach (var e in Enum.GetValues<enRedisCompletedListsNames>())
        {
            pending[e] = listKeysBatch.ListRangeAsync((e).ToString(), 0, -1);
        }

        listKeysBatch.Execute();

        await Task.WhenAll(pending.Values);

        var result = new Dictionary<enRedisCompletedListsNames, IEnumerable<string>>();

        foreach (var (key, value) in pending)
        {
            var res = await value;
            result[key] = res.Select(rv => Helpers.ExtractId(rv));
        }

        return result;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="listsKeys"></param>
    /// <returns> <see cref="Dictionary{string, IEnumerable{RedisValue}}"/> TKey is the name of the list, and the value is the list of RedisValue objects.</returns>
    public async Task<Dictionary<string, IEnumerable<RedisValue>>> ExecutePopTransactionAsync(
        IEnumerable<string> listsKeys,
        int expectedListLength = 24
    )
    {
        ArgumentNullException.ThrowIfNull(listsKeys);

        var keysArray = listsKeys as string[] ?? listsKeys.ToArray();

        if (keysArray.Length == 0)
        {
            return new Dictionary<string, IEnumerable<RedisValue>>();
        }

        if (keysArray.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException(
                "List keys cannot be null or whitespace.",
                nameof(listsKeys)
            );
        }

        try
        {
            var transaction = _redis.CreateTransaction();
            var pending = new Dictionary<string, Task<RedisValue>[]>();

            foreach (var key in listsKeys)
            {
                transaction.AddCondition(Condition.ListLengthEqual(key, expectedListLength));
                var popTasks = new Task<RedisValue>[expectedListLength];

                for (int i = 0; i < expectedListLength; i++)
                {
                    popTasks[i] = transaction.ListLeftPopAsync(key);
                }
                pending[key] = popTasks;
            }

            var committed = await transaction.ExecuteAsync();

            if (!committed)
            {
                throw new InvalidOperationException(
                    $"Redis transaction failed. One or more lists did not have exactly {expectedListLength} elements. "
                        + $"Keys: {string.Join(", ", keysArray)}"
                );
            }

            await Task.WhenAll(pending.Values.SelectMany(t => t));

            var result = new Dictionary<string, IEnumerable<RedisValue>>();

            foreach (var (key, task) in pending)
            {
                var lst = await Task.WhenAll(task);
                result[key] = lst;
            }

            return result;
        }
        catch (InvalidOperationException ex)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Log the exception here with proper context
            throw new InvalidOperationException(
                $"Failed to execute Redis pop transaction for keys: {string.Join(", ", keysArray)}",
                ex
            );
        }
    }

    public async Task<bool> RemoveKeysFromCompletedLists(IEnumerable<string> listsKeys)
    {
        if (listsKeys == null || listsKeys.Count() == 0)
            return false;
        var transaction = _redis.CreateTransaction();
        try
        {
            foreach (var key in listsKeys)
            {
                foreach (var e in Enum.GetValues<enRedisCompletedListsNames>())
                {
                    transaction.ListRemoveAsync((e).ToString(), key);
                }
            }

            var committed = await transaction.ExecuteAsync();
            return committed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing keys from completed lists.");
            throw;
        }
    }
}
