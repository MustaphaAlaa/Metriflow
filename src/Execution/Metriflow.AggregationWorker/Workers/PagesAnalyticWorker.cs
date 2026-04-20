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
        logger.LogInformation("PageAnalytics worker is started.");


        logger.LogInformation("@@@@PageAnalytics Worker is Started.");
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
                    var pagesNumber = await orc.CreatePageAnalyticsAsync();

                    // await Notify(pagesNumber);
                }
                else
                    logger.LogInformation("@@@@Records are less than zero.");
            },
            stoppingToken
        );


        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task Notify(int recordsCount)
    {
        await producer.NotifyCompletedMessageAsync(new AggregationCompletedMessage
            {
                CorrelationId = Guid.NewGuid(),
                CompletedType = AggregationType.Page,
                ProcessedCount = recordsCount,
                CompletedAt = DateTime.UtcNow,
            },
            _rabbitMqSettings.Queues.IntervalAggregation, _rabbitMqSettings.Exchange);
    }
}