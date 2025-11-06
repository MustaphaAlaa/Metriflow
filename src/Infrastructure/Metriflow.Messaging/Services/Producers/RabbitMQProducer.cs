using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Metriflow.Messaging.interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Metriflow.Messaging;

/// <summary>
/// Implements the RabbitMQ message publishing functionality.
/// </summary>
public class RabbitMQProducer : IRabbitMQProducer, IAsyncDisposable
{
    private readonly IRabbitMQConnection _connection;
    private readonly ILogger<RabbitMQProducer> _logger;
    private IChannel? _sharedChannel;

    /// <summary>
    /// Initializes a new instance of the RabbitMQProducer class.
    /// </summary>
    /// <param name="connection">The RabbitMQ connection instance.</param>
    /// <param name="logger">The logger instance for logging producer events.</param>
    public RabbitMQProducer(IRabbitMQConnection connection, ILogger<RabbitMQProducer> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task InitializeSharedChannelAsync(string exchangeName)
    {
        _sharedChannel = await _connection.CreateNewChannelAsync();
        await _sharedChannel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false
        );
        _logger.LogInformation("Shared channel created.");
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
}
