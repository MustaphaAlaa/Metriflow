using Metriflow.Application.Entities;
using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Workers;
using Microsoft.Extensions.Options;

namespace Metriflow.AggregationWorker.Workers;

public class PagesAnalyticWorker(
    ILogger<PagesAnalyticWorker> logger,
    IProducer producer,
    IMessageBrokerConsumer consumer,
    IOptions<RabbitMqSettings> options,
    IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private readonly RabbitMqSettings _rabbitMqSettings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PageId analytics worker is started.");


        logger.LogInformation("@@@@PageId Analytics Worker is Started.");
        var analyticChannel = await consumer.CreateNewChannelAsync();

        await consumer.ConsumeFromChannelAsync<AggregationCompletedMessage>(
            analyticChannel,
            _rabbitMqSettings.Queues.Correlation,
            exchangeName: _rabbitMqSettings.Exchange,
            _rabbitMqSettings.Queues.Correlation,
            async message =>
            {
                logger.LogWarning(
                    "$$$$$$$$AggregationCompletedMessage received. CorrelationCount={Count}",
                    message.ProcessedCount
                );
                if (message.ProcessedCount > 0)
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    var orc = scope.ServiceProvider.GetRequiredService<IPageAnalyticsOrchestration>();
                    await orc.CreatePageAnalyticsAsync();
                    logger.LogInformation("@@@@Oh there are data coming.");
                    
                }
                else
                    logger.LogInformation("@@@@Records are less than zero.");
             },
            stoppingToken
        );

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}