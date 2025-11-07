using Metriflow.Messaging.Entities;
using Metriflow.Messaging.interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Metriflow.Messaging;

/// <summary>
/// Implements the RabbitMQ connection management functionality.
/// </summary>
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

    public async Task<IChannel> CreateNewChannelAsync()
    {
        var _channel = await _connection.CreateChannelAsync();
        _logger.LogDebug("A new channel is created.");
        return _channel;
    }
}
