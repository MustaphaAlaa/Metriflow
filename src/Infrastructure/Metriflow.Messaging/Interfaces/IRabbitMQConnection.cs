using RabbitMQ.Client;

namespace Metriflow.Messaging.interfaces;

/// <summary>
/// Represents a connection to RabbitMQ message broker and provides methods for basic messaging operations.
/// </summary>
public interface IRabbitMQConnection  
{
    /// <summary>
    /// Creates a new channel with specified exchange configuration.
    /// </summary>
    /// <param name="exchangeName">The name of the exchange to create or use.</param>
    /// <param name="exchangeType">The type of the exchange. Defaults to "direct".</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    // Task CreateChannel(string exchangeName, string exchangeType = "direct");

    /// <summary>
    /// Releases the channel used by the current instance of IRabbitMQConnection.
    /// </summary>
    

    /// <summary>
    /// Gets a channel instance from the shared connection.
    /// This channel will be used by producers or consumers for their specific tasks.
    /// </summary>
    /// <returns>A new channel created from the connection.</returns>
    Task<IChannel> CreateNewChannelAsync();
}
