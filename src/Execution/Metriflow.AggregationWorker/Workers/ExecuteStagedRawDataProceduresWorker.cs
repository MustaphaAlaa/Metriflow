using IRepository.Generic;
using Metriflow.Application.Entities;
using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Workers;
using Microsoft.Extensions.Options;

namespace Metriflow.AggregationWorker.Workers;

public class ExecuteStagedRawDataProceduresWorker(
    ILogger<ExecuteStagedRawDataProceduresWorker> logger,
    IMessageBrokerConsumerChannels consumer,
    IProducer producer,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<RabbitMqSettings> options
) : BackgroundService
{
    private readonly RabbitMqSettings _rabbitMqSettings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = await consumer.CreateNewChannelAsync();
        await consumer.ConsumeFromChannelAsync<AggregationCompletedMessage>(
            channel,
            queueName: _rabbitMqSettings.Queues.Correlation,
            exchangeName: _rabbitMqSettings.Exchange,
            routingKey: _rabbitMqSettings.Queues.Correlation,
            handleMessage: async (AggregationCompletedMessage) =>
            {
                logger.LogInformation("");
                using var scope = serviceScopeFactory.CreateScope();
                var rawDataRepository =
                    scope.ServiceProvider.GetRequiredService<IRawDataRepository>();
                await rawDataRepository.ExecuteAnalyticsPagesCorrelationAsync();
                logger.LogInformation("");
            },
            cancellationToken: stoppingToken
        );
        
    }

    private async Task Notify(int recordsCount, CancellationToken cancellationToken)
    {
        await producer.NotifyCompletedMessageAsync(
            message: new AggregationCompletedMessage
            {
                CorrelationId = Guid.NewGuid(),
                CompletedType = AggregationType.Page,
                ProcessedCount = recordsCount,
                CompletedAt = DateTime.UtcNow,
            },
            routingKey: _rabbitMqSettings.Queues.IntervalAggregation,
            exchangeName: _rabbitMqSettings.Exchange,
            cancellationToken: cancellationToken
        );
    }
}
