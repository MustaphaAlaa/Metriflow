using System.Text;
using System.Text.Json;
using Metriflow.Correlation.Worker.Interfaces;
using Metriflow.Messaging.interfaces;
using StackExchange.Redis;

namespace Metriflow.Correlation.Worker;

/// <summary>
/// Default implementation of <see cref="IConsumerMessageHandler"/> used by the Correlation worker.
/// </summary>
/// <remarks>
/// - Logs a message (calls <see cref="ILogger{TCategoryName}.LogInformation"/>),
/// - Serializes the provided typed object to JSON using <c>JsonSetting.SerializerOptions</c>,
/// - Stores the serialized bytes into a Redis hash (field under a hash key).
/// - The implementation currently includes an artificial 1 second delay (Task.Delay(1000)) —
///   consider making this configurable or removing it for production.
/// </remarks>
public class ConsumerMessageHandler : IConsumerMessageHandler
{
    private readonly ILogger<CorrelationWorker> _logger;
    private readonly IDatabase _redis;

    /// <summary>
    /// Create a new ConsumerMessageHandler.
    /// </summary>
    /// <param name="logger">Logger instance used for informational and error logging.</param>
    /// <param name="redis">Redis connection multiplexer used to obtain a database.</param>
    public ConsumerMessageHandler(ILogger<CorrelationWorker> logger, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redis = redis.GetDatabase();
    }

    public async Task MessageHandler<T>(T obj, string message, string hashKey, string fieldName)
    {
        await Task.Delay(1000);
        _logger.LogInformation(message);

        await _redis.HashSetAsync(
            hashKey,
            [
                new(
                    fieldName,
                    Encoding.UTF8.GetBytes(
                        JsonSerializer.Serialize(obj, JsonSetting.SerializerOptions)
                    )
                ),
            ]
        );
    }
}
