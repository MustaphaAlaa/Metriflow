using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Metriflow.Correlation.Worker.Interfaces;
using Metriflow.Messaging.interfaces;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Metriflow.Correlation.Worker;

public class CorrelationWorker : BackgroundService
{
    private readonly ILogger<CorrelationWorker> _logger;
    private readonly IDatabase _redis;
    private readonly IConsumer _consumer;
    private readonly IHelper _helper;

    public CorrelationWorker(
        IHelper helper,
        IConsumer consumer,
        ILogger<CorrelationWorker> logger,
        IConnectionMultiplexer redis
    )
    {
        _helper = helper;
        _consumer = consumer;
        _logger = logger;
        _redis = redis.GetDatabase();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consume = _consumer.Consume(stoppingToken);

        var batchMatchTask = Task.Run(
            async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await _helper.MatchAll();
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken); // run every 2 seconds
                }
            },
            stoppingToken
        );

        await Task.WhenAll(consume, batchMatchTask);
    }
}
