using IRepository.Generic;
using Metriflow.Application.Entities;
using Metriflow.Application.Interfaces;
using Metriflow.Messages.Producers;
using Microsoft.Extensions.Options;

namespace Metriflow.AggregationWorker.Workers;

public class TimeIntervalAnalyticsWorker(
    ILogger<TimeIntervalAnalyticsWorker> logger,
    IMessageBrokerConsumer consumer,
    INotifyWorkers notifyWorkers,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<RabbitMqSettings> options)
    : BackgroundService
{
    private readonly RabbitMqSettings _rabbitMqSettings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("@@@@Time Interval Analytics Worker is started.");


        var analyticChannel = await consumer.CreateNewChannelAsync();

        await consumer.ConsumeFromChannelAsync<AggregationCompletedMessage>(
            analyticChannel,
            _rabbitMqSettings.Queues.IntervalAggregation,
            exchangeName: _rabbitMqSettings.Exchange,
            _rabbitMqSettings.Queues.IntervalAggregation,
            async message =>
            {
                logger.LogWarning(
                    "$$$$$$$$Aggregation Completed Message received, Interval. TimeIntervalCount={Count}",
                    message.ProcessedCount
                );
                if (message is { ProcessedCount: > 0, CompletedType: AggregationType.Interval })
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<ITimeIntervalAnalyticsRepository>();
                    var recordsCount = await repo.ExecuteAggregateTimeIntervalsAsync(stoppingToken);

                    if (recordsCount > 0)
                    {
                        await notifyWorkers.Notify(
                            recordsCount,
                            AggregationType.Interval,
                            _rabbitMqSettings.Queues.DailyAggregation,
                            stoppingToken);
                    }
                }
                else
                    logger.LogInformation("@@@@The count of records to aggregate to Interval is zero.");
            },
            stoppingToken,
            prefetchCount: 1
        );


        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}