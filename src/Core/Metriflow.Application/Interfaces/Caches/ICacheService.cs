namespace Metriflow.Application.Interfaces.Caches;

public interface ICacheService
{
    Task<long> AddFirstAsync(string key, string value);
    Task<long> AddLastAsync(string key, string value);
    Task TruncateAsync();
}