using IRepository.Generic;
using Metriflow.Application.Entities;
using Metriflow.Application.Interfaces;
using Metriflow.Messages.Producers;
using Microsoft.Extensions.Options;

namespace Metriflow.AggregationWorker.Workers;

public class PagesAnalyticWorker(
    ILogger<PagesAnalyticWorker> logger,
    IMessageBrokerConsumer consumer,
    INotifyWorkers notifyWorkers,
    IOptions<RabbitMqSettings> options,
    IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private readonly RabbitMqSettings _rabbitMqSettings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PageAnalytics worker is started.");


        logger.LogInformation("@@@@PageAnalytics Worker is Started.");

        // while (!stoppingToken.IsCancellationRequested)
        // {
        //     await Task.Delay(1000, stoppingToken);

        //     using var scope = serviceScopeFactory.CreateScope();
        //     var correlationRepository =
        //         scope.ServiceProvider.GetRequiredService<IPageAnalyticsCorrelationRepository>();
        //     var recordsCount = await correlationRepository.ExecuteAnalyticsPagesCorrelationAsync(stoppingToken);


        //     if (recordsCount > 0)
        //     {
        //         await notifyWorkers.Notify(
        //             recordsCount: 1,
        //             AggregationType.Interval,
        //             routingKey: _rabbitMqSettings.Queues.IntervalAggregation,
        //             stoppingToken);
        //     }
        // }

        
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

                if (message.ProcessedCount > 0 || message.CompletedType != AggregationType.Page)
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    var correlationRepository =
                        scope.ServiceProvider.GetRequiredService<IPageAnalyticsCorrelationRepository>();
                    var recordsCount = await correlationRepository.ExecuteAnalyticsPagesCorrelationAsync(stoppingToken);

                    if (recordsCount > 0)
                    {
                        await notifyWorkers.Notify(
                            recordsCount: 1,
                            AggregationType.Interval,
                            routingKey: _rabbitMqSettings.Queues.IntervalAggregation,
                            stoppingToken);
                    }
                }
            },
            stoppingToken,
            prefetchCount: 1
        );


        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}