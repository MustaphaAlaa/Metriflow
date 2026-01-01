using Metriflow.Application.Interfaces.Caches;
using Metriflow.Application.Models.Enums; 
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Metriflow.Redis;

public class RedisCompletedAnalyticsStore  : IAnalyticsCacheServices
{
    private readonly ILogger<RedisCompletedAnalyticsStore> _logger;
    private readonly IDatabase _redis;

    public RedisCompletedAnalyticsStore(ILogger<RedisCompletedAnalyticsStore> logger, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redis = redis.GetDatabase();
    }

    public async Task<
        Dictionary<enCompletedListsNames, IEnumerable<string>>
    > GetCompletedListsKeysAsync()
    {
        var listKeysBatch = _redis.CreateBatch();

        var pending = new Dictionary<enCompletedListsNames, Task<RedisValue[]>>();

        foreach (var e in Enum.GetValues<enCompletedListsNames>())
        {
            pending[e] = listKeysBatch.ListRangeAsync((e).ToString(), 0, -1);
        }

        listKeysBatch.Execute();

        await Task.WhenAll(pending.Values); 
        var result = new Dictionary<enCompletedListsNames, IEnumerable<string>>();
  
        foreach (var (key, value) in pending)
        {
            var res = await value;
              result[key] = res.Select(rv =>
              {
                  var st =
                      rv.ToString();
                  return st;
              }); 
        }
            
        return result;
    }

        public async Task<  Dictionary<string, IEnumerable<byte[]>> > ExecutePopTransactionAsync(
        IEnumerable<string> listsKeys,
        int expectedListLength = 24
    )
    {
        ArgumentNullException.ThrowIfNull(listsKeys);

        var keysArray = listsKeys as string[] ?? listsKeys.ToArray();

        if (keysArray.Length == 0)
        {
            return new Dictionary<string, IEnumerable<byte[]>>();
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
                // transaction.AddCondition(Condition.ListLengthEqual(key, expectedListLength));
            var  listLenght =   transaction.ListLengthAsync(key);
             
            await transaction.ExecuteAsync();
            
            if(listLenght.Result != expectedListLength)
                continue;
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

            var result = new Dictionary<string, IEnumerable<byte[]>>();

            foreach (var (key, task) in pending)
            {
                var lst = await Task.WhenAll(task);
                result[key] = lst.Select(r=> (byte[])r);//Cast<byte>(); //.Select(rv=> RedisKeyParser.ExtractId(rv.ToString()));
                int x = 3;
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
                    var prefixIndex = key.IndexOf("|");
                    var sharedKey = key[(prefixIndex+1)..];
                foreach (var e in Enum.GetValues<enCompletedListsNames>())
                {
                    var CompletedListName = e.ToString();
                    transaction.ListRemoveAsync(CompletedListName, sharedKey,0);
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
 