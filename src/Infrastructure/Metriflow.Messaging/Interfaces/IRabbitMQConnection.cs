/// <summary>
/// Represents a connection to RabbitMQ message broker and provides methods for basic messaging operations.
/// </summary>
public interface IRabbitMQConnection : IDisposable
{
    /// <summary>
    /// Creates a new channel with specified exchange configuration.
    /// </summary>
    /// <param name="exchangeName">The name of the exchange to create or use.</param>
    /// <param name="exchangeType">The type of the exchange. Defaults to "direct".</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CreateChannel(string exchangeName, string exchangeType = "direct");

    /// <summary>
    /// Releases the channel used by the current instance of IRabbitMQConnection.
    /// </summary>
    void Dispose();

    /// <summary>
    /// Publishes a message to the specified exchange with the given routing key.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message to publish.</typeparam>
    /// <param name="message">The message to publish.</param>
    /// <param name="exchangeName">The name of the exchange to publish to.</param>
    /// <param name="routingKey">The routing key for message routing.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    Task Publish<TMessage>(TMessage message, string exchangeName, string routingKey);
}

