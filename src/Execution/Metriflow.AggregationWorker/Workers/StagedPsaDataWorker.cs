using IRepository.Generic;
using Metriflow.Application.Entities;
using Metriflow.Application.Interfaces;
using Metriflow.Messages.Producers;
using Microsoft.Extensions.Options;

namespace Metriflow.AggregationWorker.Workers;

public class StagedPsaDataWorker(
    ILogger<StagedPsaDataWorker> logger,
    IMessageBrokerConsumer consumer,
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
            routingKey: _rabbitMqSettings.Queues.StagingPSA,
            exchangeName: _rabbitMqSettings.Exchange,
            queueName: _rabbitMqSettings.Queues.StagingPSA,
            handleMessage: async msg =>
            {
                logger.LogInformation(
                    "PSA staging message received. ProcessedCount={Count}, CompletedType={Type}",
                    msg.ProcessedCount,
                    msg.CompletedType);

                if (msg.ProcessedCount < 1 || msg.CompletedType != AggregationType.Records)
                    return;

                using var scope = serviceScopeFactory.CreateScope();
                var stagingRepository = scope.ServiceProvider.GetRequiredService<IPsaStagingRepository>();
                await stagingRepository.ExecuteStagePsaRecordsAsync(msg.ProcessedCount, stoppingToken);

                await notifyWorkers.Notify(
                    1,
                    AggregationType.Page,
                    _rabbitMqSettings.Queues.Correlation,
                    stoppingToken);
            },
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
        logger.LogWarning("Staged PSA Data Worker is done.");
    }
}
