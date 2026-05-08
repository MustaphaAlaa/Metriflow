using IRepository.Generic;
using Metriflow.Application.Entities;
using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Workers;
using Metriflow.Messages.Producers;
using Microsoft.Extensions.Options;

namespace Metriflow.AggregationWorker.Workers;

public class AggregationProgressWorker(
    ILogger<AggregationProgressWorker> logger,
    IMessageBrokerConsumer consumer,
    IProducer producer,
    IServiceScopeFactory serviceScopeFactory,
    INotifyWorkers notifyWorkers,
    IOptions<RabbitMqSettings> options
) : BackgroundService
{
    private readonly RabbitMqSettings _rabbitMqSettings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = await consumer.CreateNewChannelAsync();
        await consumer.ConsumeFromChannelAsync<AggregationCompletedMessage>(
            channel,
            routingKey: _rabbitMqSettings.Queues.Correlation,
            exchangeName: _rabbitMqSettings.Exchange,
            queueName: _rabbitMqSettings.Queues.Correlation,
            handleMessage: async (msg) =>
            {
                logger.LogInformation(
                    $">< Message Arrived atAggregation Worker count {msg.ProcessedCount} -- completed type {msg.CompletedType}"
                );
                if (msg.ProcessedCount < 1 || msg.CompletedType != AggregationType.Records)
                    return;
                using var scope = serviceScopeFactory.CreateScope();

                var aggregationProgressRepository =
                    scope.ServiceProvider.GetRequiredService<IRawDataRepository>();

                await aggregationProgressRepository.ExecuteStagedProcedures();
                await notifyWorkers.Notify(
                    1,
                    AggregationType.Page,
                    _rabbitMqSettings.Queues.DailyAggregation
                );
            },
            cancellationToken: stoppingToken
        );

        await Task.Delay(Timeout.Infinite, stoppingToken);
        logger.LogWarning("%%%%%%%% Aggregation Worker is Done. %%%%%%%%%%");
    }
}
