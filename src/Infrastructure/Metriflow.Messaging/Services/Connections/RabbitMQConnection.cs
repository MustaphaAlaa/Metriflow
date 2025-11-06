using Metriflow.Messaging.Entities;
using Metriflow.Messaging.interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Metriflow.Messaging;

/// <summary>
/// Represents a connection to a RabbitMQ server, providing methods to publish messages and manage channels.
/// </summary>
/// <remarks>
/// This class implements the <see cref="IRabbitMQConnection"/> interface and is responsible for establishing
/// a connection to RabbitMQ, creating channels, and publishing messages to specified exchanges.
/// </remarks>
/// <exception cref="ArgumentNullException">
/// Thrown when the <paramref name="logger"/> or <paramref name="options"/> parameters are null,
/// or when <paramref name="options.Value"/> is null.
/// </exception>
/// <example>
/// <code>
/// var rabbitMqConnection = new RabbitMQConnection(options, logger);
/// await rabbitMqConnection.Publish(message, "exchangeName", "routingKey");
/// </code>
/// </example>
///
public class RabbitMQConnection : IRabbitMQConnection, IDisposable
{
    private readonly IConnection _connection;

    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMQConnection> _logger;

    public RabbitMQConnection(
        IOptions<RabbitMqSettings> options,
        ILogger<RabbitMQConnection> logger
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (options == null)
            throw new ArgumentNullException(nameof(options));
        if (options.Value == null)
            throw new ArgumentNullException(nameof(options) + ".Value");

        _settings = options.Value;
        var factory = new ConnectionFactory()
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            VirtualHost = "/",
        };
        try
        {
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();

            _logger.LogInformation(
                $"The RabbitMQ connection is opened: {_connection.IsOpen} -- {_connection.LocalPort}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create RabbitMQ connection or channel.");
            throw;
        }
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    // public async Task Publish<TMessage>(TMessage message, string exchangeName, string routingKey)
    // {
    //     var jsonString = JsonSerializer.Serialize(message);
    //     var body = Encoding.UTF8.GetBytes(jsonString);
    //     await _channel.BasicPublishAsync(
    //         exchange: exchangeName,
    //         routingKey: routingKey,
    //         body: body
    //     );
    // }

    // public async Task CreateChannel(string exchangeName, string exchangeType = "direct")
    // {
    //     using var _channel = await _connection.CreateChannelAsync();
    //     _logger.LogDebug("The channel is created.");

    //     _logger.LogDebug($"Creating an exchange {exchangeName} -- ExchangeType: {exchangeType}");
    //     await _channel.ExchangeDeclareAsync(
    //         exchange: exchangeName,
    //         type: exchangeType,
    //         durable: true
    //     );

    //     _logger.LogDebug("The exchange is created.");
    // }

    public async Task<IChannel> CreateNewChannelAsync()
    {
        var _channel = await _connection.CreateChannelAsync();
        _logger.LogDebug("A new channel is created.");
        return _channel;
    }
}
