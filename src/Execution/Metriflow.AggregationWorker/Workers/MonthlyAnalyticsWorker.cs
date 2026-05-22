using Metriflow.AggregationWorker.Services;
using Metriflow.Application.Entities;
using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Workers;
using Microsoft.Extensions.Options;

namespace Metriflow.AggregationWorker.Workers;

public class MonthlyAnalyticsWorker(
    ILogger<MonthlyAnalyticsWorker> logger,
    IMessageBrokerConsumer consumer,
    IProducer producer,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<RabbitMqSettings> optins)
    : BackgroundService
{
    private readonly RabbitMqSettings _rabbitMqSettings = optins.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("@@@@DailyAnalyticsWorker is started.");


        var analyticChannel = await consumer.CreateNewChannelAsync();

        await consumer.ConsumeFromChannelAsync<AggregationCompletedMessage>(
            analyticChannel,
            _rabbitMqSettings.Queues.MonthlyAggregation,
            exchangeName: _rabbitMqSettings.Exchange,
            _rabbitMqSettings.Queues.MonthlyAggregation,
            async message =>
            {
                logger.LogWarning(
                    "$$$$$$$$AggregationCompletedMessage received Monthly. records received count={Count}",
                    message.ProcessedCount
                );
                if (message.ProcessedCount > 0 && message.CompletedType == AggregationType.Daily)
                {
                    logger.LogWarning(
                        "$$$$$$$$To Process received Monthly. records received count={Count}",
                        message.ProcessedCount
                    );
                    using var scope = serviceScopeFactory.CreateScope();
                    var orc = scope.ServiceProvider.GetRequiredService<IMonthlyAnalyticsOrchestrator>();
                    var pagesNumber = await orc.AggregateMonthlyAnalyticsAsync();

                    await Notify(pagesNumber, stoppingToken);
                }
                else
                    logger.LogInformation("@@@@The count of records to aggregate to monthly is zero.");
            },
            stoppingToken
        );


        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task Notify(int recordsCount, CancellationToken cancellationToken)
    {
        await producer.NotifyCompletedMessageAsync(
            message: new AggregationCompletedMessage
            {
                CorrelationId = Guid.NewGuid(),
                CompletedType = AggregationType.Monthly,
                ProcessedCount = recordsCount,
                CompletedAt = DateTime.UtcNow,
            },
            routingKey: _rabbitMqSettings.Queues.YearlyAggregation, exchangeName: _rabbitMqSettings.Exchange,
            cancellationToken: cancellationToken);
    }
}