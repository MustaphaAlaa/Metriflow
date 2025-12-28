using Metriflow.Application.Interfaces.Caches;
using Metriflow.Correlation.Worker.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Metriflow.Correlation.Worker;

public class MatcherAndProducerWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
       // var ca = stoppingToken.CanBeCanceled;
     await  Task.Delay(2222);
    }
}

public class CorrelationWorker : BackgroundService
{
    private readonly ILogger<CorrelationWorker> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory; 

    public CorrelationWorker(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<CorrelationWorker> logger
    )
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        using var scope = _serviceScopeFactory.CreateScope();
        var scopedProvider = scope.ServiceProvider;
        var redis = scopedProvider.GetRequiredService<ICacheService>();
        await redis.TruncateAsync();

        var consumer = scopedProvider.GetRequiredService<ICorrelationConsumer>();
        var consume = consumer.Consume(stoppingToken);

        await Task.WhenAll(consume);
    }
}
