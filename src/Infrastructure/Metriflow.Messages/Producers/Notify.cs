using Metriflow.Application.Entities;
using Metriflow.Application.Interfaces.Workers;
using Metriflow.Domain.CustomAttributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Metriflow.Messages.Producers;

public interface INotifyWorkers
{
    Task Notify(int recordsCount, AggregationType aggregationType, string routingKey);
}

// [ServiceRegistration(ServiceLifetime.Scoped, typeof(INotifyWorkers))]
public class NotifyWorkers(
    ILogger<NotifyWorkers> logger,
    IProducer producer,
    IOptions<RabbitMqSettings> options
) : INotifyWorkers
{
    private readonly RabbitMqSettings _rabbitMqSettings = options.Value;

    public async Task Notify(int recordsCount, AggregationType aggregationType, string routingKey)
    {
        await producer.NotifyCompletedMessageAsync(
            new AggregationCompletedMessage
            {
                CorrelationId = Guid.NewGuid(),
                CompletedType = aggregationType,
                ProcessedCount = recordsCount,
                CompletedAt = DateTime.UtcNow,
            },
            routingKey,
            _rabbitMqSettings.Exchange
        );
        logger.LogInformation(
            $">>>>> {nameof(aggregationType)} is completed, ${routingKey} is notified."
        );
    }
}
