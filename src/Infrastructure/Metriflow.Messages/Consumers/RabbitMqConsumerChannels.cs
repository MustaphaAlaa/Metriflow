using System.Text;
using System.Text.Json;
using Metriflow.Application.Interfaces;
using Metriflow.Domain.CustomAttributes;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Metriflow.Application;

[ServiceRegistration(
    Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped,
    typeof(IMessageBrokerConsumerChannels)
)]
public class RabbitMqConsumerChannels : IMessageBrokerConsumerChannels
{
    private readonly IMessageBrokerConnection _connection;
    private readonly ILogger<RabbitMqConsumerChannels> _logger;
    private readonly IHandleMessageWithRetry _handleMessageWithRetry;

    private const int MaxRetryAttempts = 3;
    private int _prefetchCount = 30;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    public RabbitMqConsumerChannels(
        IMessageBrokerConnection connection,
        IHandleMessageWithRetry handleMessageWithRetry,
        ILogger<RabbitMqConsumerChannels> logger
    )
    {
        _handleMessageWithRetry = handleMessageWithRetry;
        _connection = connection;
        _logger = logger;
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
        CancellationToken cancellationToken,
        int prefetchCount = 30
    )
    {
        this._prefetchCount = prefetchCount;
        await SetChannelSettings(channel, queueName, exchangeName, routingKey, cancellationToken);

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

                var success = await _handleMessageWithRetry.HandleMessageWithRetryAsync(
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
                    await HandlingFailedMessagesAsync(channel, queueName, args);
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
                            requeue: false
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

        // try
        // {
        //     await Task.Delay(Timeout.Infinite, cancellationToken);
        // }
        // catch (TaskCanceledException)
        // {
        //     _logger.LogInformation("Consumer stopped for queue {QueueName}", queueName);
        // }
    }

    private async Task HandlingFailedMessagesAsync(
        IChannel channel,
        string queueName,
        BasicDeliverEventArgs args
    )
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
                await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false);
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

    private async Task SetChannelSettings(
        IChannel channel,
        string queueName,
        string exchangeName,
        string routingKey,
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

        await channel.BasicQosAsync(
            prefetchSize: 0,
            (ushort)_prefetchCount,
            global: false,
            cancellationToken
        );
    }
}
