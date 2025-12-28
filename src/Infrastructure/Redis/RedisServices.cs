using Metriflow.Application.Interfaces.Caches;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Metriflow.Redis;

public class RedisServices : ICacheService
{
    private readonly ILogger<RedisServices> _logger;
    private readonly IDatabase _redis;

    public RedisServices(ILogger<RedisServices> logger, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redis = redis.GetDatabase();
    }

    public async Task TruncateAsync()
    {
        await _redis.ExecuteAsync("FLUSHDB");
    }
    public async Task<long> AddFirstAsync(string key, string value)
    {
        try
        {
          var length =   await _redis.ListLeftPushAsync(key, value);
          return length;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }
    
    public async Task<long> AddLastAsync(string key, string value)
    {
        try
        {
             
          var length =   await _redis.ListRightPushAsync(key, value);
          return length;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }
    
    
}