using Metriflow.Messaging.interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Metriflow.Correlation.Worker;

public class CorrelationWorker : BackgroundService
{
    private readonly ILogger<CorrelationWorker> _logger;
    private readonly IRabbitMQConsumer _consumer; 

    public CorrelationWorker(
        ILogger<CorrelationWorker> logger,
        IRabbitMQConsumer consumer 
    )
    {
        _consumer = consumer;
        _logger = logger; 
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var analyticGA = await _consumer.CreateNewChannelAsync();
        var analyticPSI = await _consumer.CreateNewChannelAsync();

        var psiTask = _consumer.ConsumeFromChannelAsync(
            analyticPSI,
            queueName: "PSI-Queue",
            exchangeName: "analytics.raw",
            routingKey: "analytics.raw.psi",
            async (PSIRecord pa) =>
            {
                await Task.Delay(1000);
                _logger.LogInformation($"MESSAGE FROM CONSUMER ---- PSI => {pa}");
               
            },
            stoppingToken
        );

        var gaTask = _consumer.ConsumeFromChannelAsync(
            analyticGA,
            queueName: "GA-Queue",
            exchangeName: "analytics.raw",
            routingKey: "analytics.raw.ga",
            async (GARecord ga) =>
            {
              
                await Task.Delay(1000);
                _logger.LogInformation($"MESSAGE FROM CONSUMER ---- GA => {ga}");
            },
            stoppingToken
        );

        await Task.WhenAll(psiTask, gaTask);
    }
}
