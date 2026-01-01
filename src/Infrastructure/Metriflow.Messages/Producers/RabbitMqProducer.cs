using System.Runtime.CompilerServices;
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
public class RabbitMqProducer : IMessageBrokerProducer, IAsyncDisposable
{
    private readonly IMessageBrokerConnection _connection;
    private readonly ILogger<RabbitMqProducer> _logger;
    private IChannel? _sharedChannel;

    /// <summary>
    /// Initializes a new instance of the RabbitMqProducer class.
    /// </summary>
    /// <param name="connection">The RabbitMQ connection instance.</param>
    /// <param name="logger">The logger instance for logging producer events.</param>
    public RabbitMqProducer(IMessageBrokerConnection connection, ILogger<RabbitMqProducer> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task InitializeSharedChannelAsync(string exchangeName)
    {
        if (_sharedChannel is null)
        {
            _sharedChannel = await _connection.CreateNewChannelAsync();
            _logger.LogInformation("Shared channel created.");
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
        var channel = await _connection.CreateNewChannelAsync();

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
        string routingKey
    )
    {
        if (_sharedChannel is null)
            throw new InvalidOperationException("Shared channel not initialized.");

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        await _sharedChannel.BasicPublishAsync(exchange, routingKey, body: body);
    }

    public async Task PublishWithNewChannelAsync<T>(
        T message,
        string exchangeName,
        string routingKey
    )
    {
        using var channel = await _connection.CreateNewChannelAsync();
        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false
        );
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        await channel.BasicPublishAsync(exchangeName, routingKey, body: body);
    }

    public async ValueTask DisposeAsync()
    {
        if (_sharedChannel != null)
            await _sharedChannel.CloseAsync();
    }

    public async Task PublishAsync<T>(T message, string exchangeName, string routingKey, bool sharedChannel = false)
    {
        if (sharedChannel)
        {
             await this.InitializeSharedChannelAsync(exchangeName);
            await this.PublishWithSharedChannelAsync(message, exchangeName, routingKey);
        } 
        else  await this.PublishWithNewChannelAsync(message, exchangeName, routingKey);
    }
}
