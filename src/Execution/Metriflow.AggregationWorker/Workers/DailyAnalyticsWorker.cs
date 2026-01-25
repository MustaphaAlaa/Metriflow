using Metriflow.AggregationWorker.Services;
using Metriflow.Application.Entities;
using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Workers;
using Metriflow.Domain.CustomAttributes;
using Microsoft.Extensions.Options;

namespace Metriflow.AggregationWorker.Workers;

public class DailyAnalyticsWorker(ILogger<DailyAnalyticsWorker> logger, IMessageBrokerConsumer consumer,
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
            _rabbitMqSettings.Queues.DailyAggregation,
            exchangeName: _rabbitMqSettings.Exchange,
            _rabbitMqSettings.Queues.DailyAggregation,
            async message =>
            {
                logger.LogWarning(
                    "$$$$$$$$AggregationCompletedMessage received Daily. records received count={Count}",
                    message.ProcessedCount
                );
                if (message.ProcessedCount > 0 && message.CompletedType == AggregationType.Interval)
                {
                    logger.LogWarning(
                        "$$$$$$$$To Process received Daily. records received count={Count}",
                        message.ProcessedCount
                    );
                    using var scope = serviceScopeFactory.CreateScope();
                    var orc = scope.ServiceProvider.GetRequiredService<IDailyAnalyticsOrchestrator>();
                    var pagesNumber = await orc.AggregateDailyAnalyticsAsync();

                    await Notify(pagesNumber);
                }
                else
                    logger.LogInformation("@@@@The count of records to aggregate to dailies is zero.");
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
            CompletedType = AggregationType.Daily,
            ProcessedCount = recordsCount,
            CompletedAt = DateTime.UtcNow,
        },
            _rabbitMqSettings.Queues.MonthlyAggregation, _rabbitMqSettings.Exchange);
    }
}