namespace Metriflow.Application.Entities;

/// <summary>
/// Configuration settings for RabbitMQ connection.
/// </summary>
public class RabbitMqSettings
{
    /// <summary>
    /// The hostname of the RabbitMQ server.
    /// </summary>
    public string HostName { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;

    /// <summary>
    /// The port number on which RabbitMQ server is running.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Username for authentication with RabbitMQ server.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Password for authentication with RabbitMQ server.
    /// </summary>
    public string Password { get; set; } = string.Empty;
    public QueueSettings Queues { get; set; } = new();

}