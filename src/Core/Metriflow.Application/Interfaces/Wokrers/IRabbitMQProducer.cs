using RabbitMQ.Client;

namespace Metriflow.Application.interfaces;

/// <summary>
/// Interface defining the contract for publishing messages to RabbitMQ.
/// </summary>
public interface IRabbitMQProducer
{
    /// <summary>
    /// Disposes of the producer resources asynchronously.
    /// </summary>
    ValueTask DisposeAsync();

    /// <summary>
    /// Initializes a shared channel with the specified exchange.
    /// </summary>
    /// <param name="exchangeName">The name of the exchange to declare.</param>
    Task InitializeSharedChannelAsync(string exchangeName);

    /// <summary>
    /// Publishes a message using a new channel that is created and disposed for each publish operation.
    /// </summary>
    /// <typeparam name="T">The type of the message to publish.</typeparam>
    /// <param name="message">The message to publish.</param>
    /// <param name="exchangeName">The exchange to publish to.</param>
    /// <param name="routingKey">The routing key for message routing.</param>
    Task PublishWithNewChannelAsync<T>(T message, string exchangeName, string routingKey);

    /// <summary>
    /// Publishes a message using the shared channel.
    /// </summary>
    /// <typeparam name="T">The type of the message to publish.</typeparam>
    /// <param name="message">The message to publish.</param>
    /// <param name="exchangeName">The exchange to publish to.</param>
    /// <param name="routingKey">The routing key for message routing.</param>
    Task PublishWithSharedChannelAsync<T>(T message, string exchangeName, string routingKey);

    /// <summary>
    /// Creates a new channel with the specified exchange declared.
    /// </summary>
    /// <param name="exchangeName">The name of the exchange to declare.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an IChannel instance.</returns>
    Task<IChannel> CreateNewChannelAsync(string exchangeName);

    /// <summary>
    /// Publishes a message to a specific channel.
    /// </summary>
    /// <typeparam name="T">The type of the message to publish.</typeparam>
    /// <param name="channel">The channel to publish through.</param>
    /// <param name="message">The message to publish.</param>
    /// <param name="exchangeName">The exchange to publish to.</param>
    /// <param name="routingKey">The routing key for message routing.</param>
    Task PublishToChannelAsync<T>(
        IChannel channel,
        T message,
        string exchangeName,
        string routingKey
    );
}
