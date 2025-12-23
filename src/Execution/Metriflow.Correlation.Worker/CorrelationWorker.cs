using Metriflow.Correlation.Worker.Interfaces;
using StackExchange.Redis;

namespace Metriflow.Correlation.Worker;


public class MatcherAndProducerWorker : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}
public class CorrelationWorker : BackgroundService
{
    private readonly ILogger<CorrelationWorker> _logger;
    private readonly IDatabase _redis;
    private readonly ICorrelationConsumer _consumer;

    public CorrelationWorker(
        ICorrelationConsumer consumer,
        ILogger<CorrelationWorker> logger,
        IConnectionMultiplexer redis
    )
    {
        _consumer = consumer;
        _logger = logger;
        _redis = redis.GetDatabase();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _redis.ExecuteAsync("FLUSHDB");

        var consume = _consumer.Consume(stoppingToken);

        await Task.WhenAll(consume);
    }
}
