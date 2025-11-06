using System.Text;
using System.Text.Json;
using Metriflow.Messaging.interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Metriflow.Messaging;

/// <summary>
/// RabbitMQ consumer implementation that handles message consumption using
/// RabbitMQ channels created from an injected <see cref="IRabbitMQConnection"/>.
/// </summary>
/// <remarks>
/// - Declares the exchange and queue (durable, non-auto-delete) and binds them
///   using the provided routing key prior to consuming.
/// - Uses <see cref="AsyncEventingBasicConsumer"/> and deserializes payloads as UTF-8 JSON
///   using <see cref="System.Text.Json.JsonSerializer"/> into the generic <typeparamref name="T"/>.
/// - After successful processing the consumer acks the delivery; on exception it nacks with requeue=true.
/// - The shared-channel flow requires calling <see cref="InitializeSharedChannelAsync"/> before
///   <see cref="ConsumeWithSharedChannelAsync{T}"/>.
/// - Cancellation is honored via the provided <see cref="CancellationToken"/> passed to <see cref="ConsumeFromChannelAsync{T}"/>
///   (the method awaits an infinite delay that completes when the token is canceled).
/// </remarks>
public class RabbitMQConsumer : IAsyncDisposable, IRabbitMQConsumer
{
    private readonly IRabbitMQConnection _connection;
    private readonly ILogger<RabbitMQConsumer> _logger;
    private IChannel? _sharedChannel;

    /// <summary>
    /// Create a new <see cref="RabbitMQConsumer"/>.
    /// </summary>
    /// <param name="connection">Connection provider used to create channels.</param>
    /// <param name="logger">Logger used to record lifecycle and error events.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="connection"/> or <paramref name="logger"/> is null.</exception>
    public RabbitMQConsumer(IRabbitMQConnection connection, ILogger<RabbitMQConsumer> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task InitializeSharedChannelAsync(string queueName, string exchangeName)
    {
        _sharedChannel = await _connection.CreateNewChannelAsync();
        _logger.LogInformation(
            $"The shared channel is Created. Channel Number: {_sharedChannel.ChannelNumber}"
        );
    }

    /// <inheritdoc/>
    public async Task ConsumeWithSharedChannelAsync<T>(
        string queueName,
        string exchangeName,
        string routingKey,
        Func<T, Task> handleMessage,
        CancellationToken cancellationToken
    )
    {
        if (_sharedChannel is null)
            throw new InvalidOperationException("Shared channel not initialized.");

        await this.ConsumeFromChannelAsync(
            channel: _sharedChannel,
            exchangeName: exchangeName,
            queueName: queueName,
            routingKey: routingKey,
            handleMessage: handleMessage,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc/>
    public async Task ConsumeWithNewChannelAsync<T>(
        string queueName,
        T message,
        string exchangeName,
        string routingKey,
        Func<T, Task> handleMessage,
        CancellationToken cancellationToken
    )
    {
        var channel = await this.CreateNewChannelAsync();

        await this.ConsumeFromChannelAsync(
            channel: channel,
            exchangeName: exchangeName,
            queueName: queueName,
            routingKey: routingKey,
            handleMessage: handleMessage,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc/>
    public async Task<IChannel> CreateNewChannelAsync()
    {
        var channel = await _connection.CreateNewChannelAsync();
        _logger.LogInformation(
            $"A new channel is Created. Channel Number: {channel.ChannelNumber}"
        );
        return channel;
    }

    /// <inheritdoc/>
    public async Task ConsumeFromChannelAsync<T>(
        IChannel channel,
        string queueName,
        string exchangeName,
        string routingKey,
        Func<T, Task> handleMessage,
        CancellationToken cancellationToken
    )
    {
        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false
        );

        // Declare queue if it doesn’t exist
        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        // Bind queue to exchange using routing key
        await channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: routingKey
        );

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());
                var message = JsonSerializer.Deserialize<T>(json);

                if (message == null)
                {
                    _logger.LogWarning(
                        "Invalid or null message received from queue {QueueName}",
                        queueName
                    );
                    return;
                }

                await handleMessage(message);

                await channel.BasicAckAsync(args.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from queue {QueueName}", queueName);

                await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true);
            }
        };

        await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer);
        _logger.LogInformation("Started consuming from queue: {QueueName}", queueName);

        try
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Consumer stopped for queue {QueueName}", queueName);
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_sharedChannel != null)
            await _sharedChannel.CloseAsync();
    }
}
