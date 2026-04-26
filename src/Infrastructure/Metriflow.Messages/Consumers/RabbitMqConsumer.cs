using System.Text;
using System.Text.Json;
using Metriflow.Application.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Metriflow.Application;

/// <summary>
/// RabbitMQ consumer implementation that handles message consumption using
/// RabbitMQ channels created from an injected <see cref="IMessageBrokerConnection"/>.
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
public class RabbitMqConsumer : IAsyncDisposable, IMessageBrokerConsumer
{
    private readonly IMessageBrokerConnection _connection;
    private readonly ILogger<RabbitMqConsumer> _logger;
    private IChannel? _sharedChannel;

    private const int MaxRetryAttempts = 3;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Create a new <see cref="RabbitMqConsumer"/>.
    /// </summary>
    /// <param name="connection">Connection provider used to create channels.</param>
    /// <param name="logger">Logger used to record lifecycle and error events.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="connection"/> or <paramref name="logger"/> is null.</exception>
    public RabbitMqConsumer(IMessageBrokerConnection connection, ILogger<RabbitMqConsumer> logger)
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
            autoDelete: false,
            cancellationToken: cancellationToken
        );
        _logger.LogDebug($"Exchange is declared: {exchangeName}");

        // Declare queue if it doesn’t exist
        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken
        );

        _logger.LogDebug($"Queue is declared: {queueName}");

        // Bind queue to exchange using routing key
        await channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: routingKey,
            cancellationToken: cancellationToken
        );
        _logger.LogDebug(
            $"Queue is bind: {queueName}, Exchange: {exchangeName}, routingKey: {routingKey}"
        );
        _logger.LogInformation(
            $"Queue is bind: {queueName}, Exchange: {exchangeName}, routingKey: {routingKey}"
        );

        var consumer = new AsyncEventingBasicConsumer(channel);
        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 300,
            global: false,
            cancellationToken
        );
        consumer.ReceivedAsync += async (sender, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());
                var message = JsonSerializer.Deserialize<T>(json);

                if (message == null)
                {
                    _logger.LogWarning(
                        "Invalid or null message received from queue {QueueName}. **NACK with Requeue=false.**",
                        queueName
                    );
                    if (channel.IsOpen)
                        await channel.BasicNackAsync(
                            args.DeliveryTag,
                            multiple: false,
                            requeue: false
                        );
                    else
                        throw new Exception("Invalid or null message received from queue");
                    return;
                }

                var success = await this.HandleMessageWithRetryAsync(
                    message,
                    handleMessage,
                    queueName,
                    cancellationToken
                );

                if (success)
                {
                    if (channel.IsOpen)
                        await channel.BasicAckAsync(args.DeliveryTag, multiple: false);
                    else
                        _logger.LogWarning(
                            "Channel is closed, cannot ack message from queue {QueueName}",
                            queueName
                        );
                }
                else
                {
                    _logger.LogError(
                        "All {Attempts} attempts failed for message from queue {QueueName}. **NACK with Requeue=true.**",
                        MaxRetryAttempts,
                        queueName
                    );
                    if (channel.IsOpen)
                    {
                        try
                        {
                            await channel.BasicNackAsync(
                                args.DeliveryTag,
                                multiple: false,
                                requeue: false
                            );
                            _logger.LogWarning(
                                "There's no DLQ is set for now, so I set requeue to false to avoid infinite redelivery"
                            );
                        }
                        catch (Exception nackEx)
                        {
                            _logger.LogError(
                                nackEx,
                                "Failed to nack message from queue {QueueName}",
                                queueName
                            );
                        }
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Channel is closed, cannot nack message from queue {QueueName}",
                            queueName
                        );
                    }
                }
            }
            catch (OperationCanceledException ocEx)
            {
                _logger.LogWarning(
                    ocEx,
                    "Message processing was canceled for queue {QueueName}. Channel state: {IsOpen}",
                    queueName,
                    channel.IsOpen
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from queue {QueueName}", queueName);

                if (channel.IsOpen)
                {
                    try
                    {
                        await channel.BasicNackAsync(
                            args.DeliveryTag,
                            multiple: false,
                            requeue: true
                        );
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning(
                            "Cancellation during nack for queue {QueueName}. Channel state: {IsOpen}",
                            queueName,
                            channel.IsOpen
                        );
                    }
                    catch (Exception nackEx)
                    {
                        _logger.LogError(
                            nackEx,
                            "Failed to nack message from queue {QueueName}",
                            queueName
                        );
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "Channel is already closed. Cannot nack message from queue {QueueName}",
                        queueName
                    );
                }
            }
        };

        await channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer,
            cancellationToken
        );
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

    /// <summary>
    /// Attempts to process a message with a defined number of retries and a delay between attempts.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="message">The deserialized message payload.</param>
    /// <param name="handleMessage">The external function to process the message (business logic).</param>
    /// <param name="queueName">The name of the queue for logging purposes.</param>
    /// <param name="cancellationToken">Cancellation token to respect shutdown requests.</param>
    /// <returns>True if the message was successfully handled, false otherwise.</returns>
    private async Task<bool> HandleMessageWithRetryAsync<T>(
        T message,
        Func<T, Task> handleMessage,
        string queueName,
        CancellationToken cancellationToken
    )
    {
        for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
        {
            try
            {
                _logger.LogInformation(
                    $"Attempt {attempt}/{MaxRetryAttempts} to process message from queue {queueName}"
                );

                await handleMessage(message);

                // Success: Break the loop and return true
                _logger.LogInformation(
                    "Message processed successfully on attempt {Attempt} from queue {QueueName}.",
                    attempt,
                    queueName
                );
                return true;
            }
            catch (OperationCanceledException)
            {
                // Cancellation requested - stop retrying
                _logger.LogInformation(
                    "Message processing canceled for queue {QueueName}.",
                    queueName
                );
                return false;
            }
            catch (ObjectDisposedException ex)
            {
                // Service provider is disposed, likely during shutdown - stop retrying
                _logger.LogWarning(
                    ex,
                    "Service provider was disposed while processing message from queue {QueueName}. Application may be shutting down. Stopping retries.",
                    queueName
                );
                return false;
            }
            catch (Exception ex) when (attempt < MaxRetryAttempts)
            {
                // Log the failure, but only if we have more retries left
                _logger.LogWarning(
                    ex,
                    "Message processing failed on attempt {Attempt} from queue {QueueName}. Retrying in {Delay} seconds...",
                    attempt,
                    queueName,
                    RetryDelay.TotalSeconds
                );

                // Wait for the delay before the next attempt
                try
                {
                    await Task.Delay(RetryDelay, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    // If cancellation is requested while waiting, stop retrying
                    _logger.LogInformation(
                        "Retry delay canceled for queue {QueueName}.",
                        queueName
                    );
                    return false;
                }
            }
            catch (Exception finalEx)
            {
                // Final failure after the last attempt
                _logger.LogError(
                    finalEx,
                    "Final attempt {Attempt} failed to process message from queue {QueueName}. No more retries.",
                    attempt,
                    queueName
                );
                return false;
            }
        }

        return false; // Should not be reached, but included for completeness
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_sharedChannel != null)
            await _sharedChannel.CloseAsync();
    }
}
