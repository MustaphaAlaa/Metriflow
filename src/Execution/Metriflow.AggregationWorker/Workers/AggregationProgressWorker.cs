using IRepository.Generic;
using Metriflow.Application.Entities;
using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Workers;
using Microsoft.Extensions.Options;

namespace Metriflow.AggregationWorker.Workers;

public class AggregationProgressWorker(
    ILogger<AggregationProgressWorker> logger,
    IMessageBrokerConsumer consumer,
    IProducer producer,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<RabbitMqSettings> optins) : BackgroundService
{
    private readonly RabbitMqSettings _rabbitMqSettings = optins.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000 * 20);
            using var scope = serviceScopeFactory.CreateScope();

            var aggregationProgressRepository =
                scope.ServiceProvider.GetRequiredService<IAggregationProgressRepository>();

            var affectedRows = await aggregationProgressRepository.InsertMissingAggregationProgressesAsync();
            await Notify(affectedRows);
        }
    }


    private async Task Notify(int recordsCount)
    {
        await producer.NotifyCompletedMessageAsync(new AggregationCompletedMessage
        {
            CorrelationId = Guid.NewGuid(),
            CompletedType = AggregationType.Records,
            ProcessedCount = recordsCount,
            CompletedAt = DateTime.UtcNow,
        },
            _rabbitMqSettings.Queues.Correlation, _rabbitMqSettings.Exchange);
    }
}