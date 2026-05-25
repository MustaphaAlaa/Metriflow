using RabbitMQ.Client;

namespace Metriflow.Messages.Producers;

/// <summary>
/// Defines the contract for configuring message broker queue and exchange bindings.
/// </summary>
public interface IMessageBrokerBinding
{
    /// <summary>
    /// Declares an exchange and queue, then binds them together using a routing key.
    /// </summary>
    /// <param name="channel">The RabbitMQ channel.</param>
    /// <param name="queueName">The name of the queue to declare and bind.</param>
    /// <param name="exchangeName">The name of the exchange to declare.</param>
    /// <param name="routingKey">The routing key for binding the queue to the exchange.</param>
    /// <param name="stoppingToken">Cancellation token for async operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task BindQueueToExchangeAsync(
        IChannel channel,
        string queueName,
        string exchangeName,
        string routingKey,
        CancellationToken stoppingToken
    );
}
