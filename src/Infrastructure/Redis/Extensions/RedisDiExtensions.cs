 
using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Caches;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Metriflow.Redis.Extensions;


public static class RedisDiExtensions
{
    public static IServiceCollection AddRedisDI(this IServiceCollection services,IConfigurationManager configuration)
    {
        services.AddScoped<IAnalyticRecordsDeserializer, RedisAnalyticRecordDeserializer>();
        services.AddScoped<IAnalyticsCacheServices, RedisCompletedAnalyticsStore>();
        services.AddScoped<ICacheService, RedisServices>();
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisConnection = configuration.GetConnectionString("Redis");
            var options = ConfigurationOptions.Parse(redisConnection);
            options.AbortOnConnectFail = true;
            return ConnectionMultiplexer.Connect(options);
        });
        
        return services;
    }
} 