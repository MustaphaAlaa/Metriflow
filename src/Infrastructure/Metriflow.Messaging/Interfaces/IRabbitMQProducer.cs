using RabbitMQ.Client;

namespace Metriflow.Messaging.interfaces;

public interface IRabbitMQProducer
{
    ValueTask DisposeAsync();
    Task InitializeSharedChannelAsync(string exchangeName);
    Task PublishWithNewChannelAsync<T>(T message, string exchangeName, string routingKey);
    Task PublishWithSharedChannelAsync<T>(T message, string exchangeName, string routingKey);

    Task<IChannel> CreateNewChannelAsync(string exchangeName);

    Task PublishToChannelAsync<T>(
        IChannel channel,
        T message,
        string exchangeName,
        string routingKey
    );
}
