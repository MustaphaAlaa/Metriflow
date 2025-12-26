using RabbitMQ.Client;

namespace Metriflow.Application.interfaces;

/// <summary>
/// Interface defining the contract for managing RabbitMQ connections.
/// </summary>
public interface IMessageBrokerConnection
{
    /// <summary>
    /// Creates a new RabbitMQ channel asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an IChannel instance.</returns>
    Task<IChannel> CreateNewChannelAsync();
}
