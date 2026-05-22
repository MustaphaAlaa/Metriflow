using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Metriflow.Messages.Producers;

/// <summary>
/// Handles the configuration of message broker queue and exchange bindings.
/// Implements clean architecture by providing a domain-focused interface for message broker operations.
/// </summary>
public class MessageBrokerBinding(ILogger<MessageBrokerBinding> logger) : IMessageBrokerBinding
{

    public async Task BindQueueToExchangeAsync(
        IChannel channel,
        string queueName,
        string exchangeName,
        string routingKey,
        CancellationToken cancellationToken
    )
    {
        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken
        );
        logger.LogDebug($"Exchange is declared: {exchangeName}");

         await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken
        );

        logger.LogDebug($"Queue is declared: {queueName}");

         await channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: routingKey,
            cancellationToken: cancellationToken
        );
        logger.LogDebug(
            $"Queue is bind: {queueName}, Exchange: {exchangeName}, routingKey: {routingKey}"
        );
        logger.LogInformation(
            $"Queue is bind: {queueName}, Exchange: {exchangeName}, routingKey: {routingKey}"
        );
    }
}