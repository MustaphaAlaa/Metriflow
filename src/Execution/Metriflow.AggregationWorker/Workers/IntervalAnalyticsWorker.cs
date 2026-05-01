using Metriflow.AggregationWorker.Services;
using Metriflow.Application.Entities;
using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Workers;
using Microsoft.Extensions.Options;

namespace Metriflow.AggregationWorker.Workers;

public class IntervalAnalyticsWorker(ILogger<IntervalAnalyticsWorker> logger,
    IMessageBrokerConsumer consumer,
    IProducer producer,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<RabbitMqSettings> optins)
    : BackgroundService
{
    private readonly RabbitMqSettings _rabbitMqSettings= optins.Value;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("@@@@IntervalAnalyticsWorker is started.");

 
        var analyticChannel = await consumer.CreateNewChannelAsync();

        await consumer.ConsumeFromChannelAsync<AggregationCompletedMessage>(
            analyticChannel,
            _rabbitMqSettings.Queues.IntervalAggregation,
            exchangeName: _rabbitMqSettings.Exchange,
            _rabbitMqSettings.Queues.IntervalAggregation,
            async message =>
            {
                logger.LogWarning(
                    "$$$$$$$$AggregationCompletedMessage received Interval. TimeIntervalCount={Count}",
                    message.ProcessedCount
                );
                if (message.ProcessedCount > 0 && message.CompletedType== AggregationType.Page)
                {
                    logger.LogWarning(
                        "$$$$$$$$To Proccess received Interval. TimeIntervalCount={Count}",
                        message.ProcessedCount
                    );
                    using var scope = serviceScopeFactory.CreateScope();
                    var orc = scope.ServiceProvider.GetRequiredService<ITimeIntervalsOrchestration>();
                    var pagesNumber = await orc.AggregateTimeIntervalsAsync();

                    await Notify(pagesNumber);
                }
                else
                    logger.LogInformation("@@@@The count of records to aggregate to Interval is zero.");
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
                CompletedType = AggregationType.Interval,
                ProcessedCount = recordsCount,
                CompletedAt = DateTime.UtcNow,
            },
            _rabbitMqSettings.Queues.DailyAggregation, _rabbitMqSettings.Exchange);
    }
}