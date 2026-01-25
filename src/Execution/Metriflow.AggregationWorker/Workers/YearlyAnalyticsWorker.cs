using Metriflow.Application.Entities;
using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Workers;
using Microsoft.Extensions.Options;

namespace Metriflow.AggregationWorker.Workers;

public class YearlyAnalyticsWorker(ILogger<YearlyAnalyticsWorker> logger,
    IMessageBrokerConsumer consumer,
    IProducer producer,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<RabbitMqSettings> options)
    : BackgroundService
{
    private readonly RabbitMqSettings _rabbitMqSettings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("@@@@YearlyAnalyticsWorker is started.");

        var analyticChannel = await consumer.CreateNewChannelAsync();

        await consumer.ConsumeFromChannelAsync<AggregationCompletedMessage>(
            analyticChannel,
            _rabbitMqSettings.Queues.YearlyAggregation,
            exchangeName: _rabbitMqSettings.Exchange,
            _rabbitMqSettings.Queues.YearlyAggregation,
            async message =>
            {
                logger.LogWarning(
                    "$$$$$$$$AggregationCompletedMessage received Yearly. records received count={Count}",
                    message.ProcessedCount
                );
                if (message.ProcessedCount > 0 && message.CompletedType == AggregationType.Monthly)
                {
                    logger.LogWarning(
                        "$$$$$$$$To Process received Yearly. records received count={Count}",
                        message.ProcessedCount
                    );
                    using var scope = serviceScopeFactory.CreateScope();
                    var orc = scope.ServiceProvider.GetRequiredService<IYearlyAnalyticsOrchestrator>();
                    var recordsCount = await orc.AggregateYearlyAnalyticsAsync();

                    // await Notify(recordsCount);
                }
                else
                    logger.LogInformation("@@@@The count of records to aggregate to yearly is zero.");
            },
            stoppingToken
        );

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    // private async Task Notify(int recordsCount)
    // {
    //     await producer.NotifyCompletedMessageAsync(new AggregationCompletedMessage
    //     {
    //         CorrelationId = Guid.NewGuid(),
    //         CompletedType = AggregationType.Yearly,
    //         ProcessedCount = recordsCount,
    //         CompletedAt = DateTime.UtcNow,
    //     },
    //         _rabbitMqSettings.Queues.YearlyAggregation, _rabbitMqSettings.Exchange);
    // }
}