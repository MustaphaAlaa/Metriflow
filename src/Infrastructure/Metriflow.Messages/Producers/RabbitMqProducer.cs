using System.Text;
using System.Text.Json;
using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Workers;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Metriflow.Messages.Producers;

/// <summary>
/// Implements the RabbitMQ message publishing functionality.
/// </summary>
public class RabbitMqProducer(
    IMessageBrokerConnection connection,
    ILogger<RabbitMqProducer> logger,
    IMessageBrokerBinding messageBrokerBinding)
    : IMessageBrokerProducer, IAsyncDisposable
{
    private IChannel? _sharedChannel;

    public async Task InitializeSharedChannelAsync(string exchangeName)
    {
        if (_sharedChannel is null)
        {
            _sharedChannel = await connection.CreateNewChannelAsync();
            logger.LogInformation("Shared channel created.");
        }

        await _sharedChannel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false
        );
    }

    public async Task<IChannel> CreateNewChannelAsync(string exchangeName)
    {
        var channel = await connection.CreateNewChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false
        );
        return channel;
    }

    public async Task PublishToChannelAsync<T>(
        IChannel channel,
        T message,
        string exchange,
        string routingKey
    )
    {
        if (channel is null)
            throw new InvalidOperationException("Shared channel not initialized.");

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        await channel.BasicPublishAsync(exchange, routingKey, body: body);
    }

    public async Task PublishWithSharedChannelAsync<T>(
        T message,
        string exchange,
        string routingKey,
        CancellationToken stoppingToken
    )
    {
        if (_sharedChannel is null)
            throw new InvalidOperationException("Shared channel not initialized.");


        await messageBrokerBinding.BindQueueToExchangeAsync(channel: _sharedChannel, exchangeName: exchange,
            routingKey: routingKey,
            queueName: routingKey,
            stoppingToken: stoppingToken);


        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        await _sharedChannel.BasicPublishAsync(exchange, routingKey, body: body);
    }

    public async Task PublishWithNewChannelAsync<T>(
        T message,
        string exchangeName,
        string routingKey,
        CancellationToken stoppingToken
    )
    {
        using var channel = await connection.CreateNewChannelAsync();


        await messageBrokerBinding.BindQueueToExchangeAsync(channel: channel, exchangeName: exchangeName,
            routingKey: routingKey,
            queueName: routingKey,
            stoppingToken: stoppingToken);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        await channel.BasicPublishAsync(exchangeName, routingKey, body: body);
    }

    public async ValueTask DisposeAsync()
    {
        if (_sharedChannel != null)
            await _sharedChannel.CloseAsync();
    }

    public async Task PublishAsync<T>(
        T message,
        string exchangeName,
        string routingKey,
        CancellationToken stoppingToken,
        bool sharedChannel = false
    )
    {
        if (sharedChannel)
        {
            await this.InitializeSharedChannelAsync(exchangeName);
            await this.PublishWithSharedChannelAsync(message, exchangeName, routingKey, stoppingToken);
        }
        else
            await this.PublishWithNewChannelAsync(message, exchangeName, routingKey, stoppingToken);
    }
}
 