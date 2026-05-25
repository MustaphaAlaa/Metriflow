using RabbitMQ.Client;

namespace Metriflow.Application.Interfaces;

public interface IMessageBrokerConsumerChannels
{
    /// <summary>
    /// Consume messages of type <typeparamref name="T"/> using the supplied channel.
    /// </summary>
    /// <typeparam name="T">The CLR type to which the message payload will be deserialized.</typeparam>
    /// <param name="channel">An open AMQP channel instance to use for consuming messages.</param>
    /// <param name="queueName">The queue name to consume from. The implementation may declare/bind the queue if necessary.</param>
    /// <param name="exchangeName">The exchange name to bind the queue to if required by the implementation.</param>
    /// <param name="routingKey">The routing key to use when binding or filtering messages.</param>
    /// <param name="handleMessage">Async callback invoked for each successfully deserialized message.</param>
    /// <param name="cancellationToken">Token used to stop consuming and to abort long-running operations.</param>
    /// <returns>A <see cref="Task"/> that completes when consumption has stopped (for example when <paramref name="cancellationToken"/> is cancelled or the channel closes).</returns>
    /// <exception cref="ArgumentNullException">Thrown if required arguments are null.</exception>
    /// <exception cref="OperationCanceledException">May be thrown when <paramref name="cancellationToken"/> is cancelled.</exception>
    Task ConsumeFromChannelAsync<T>(
        IChannel channel,
        string queueName,
        string exchangeName,
        string routingKey,
        Func<T, Task> handleMessage,
        CancellationToken cancellationToken,
        int prefetchCount = 30
    );

    /// <summary>
    /// Create a new AMQP channel.
    /// </summary>
    /// <returns>A task that resolves to a fresh <see cref="IChannel"/> instance. Caller is responsible for channel lifetime.</returns>
    Task<IChannel> CreateNewChannelAsync();
}
