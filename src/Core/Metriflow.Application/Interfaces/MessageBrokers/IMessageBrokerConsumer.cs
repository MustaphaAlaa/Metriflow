namespace Metriflow.Application.Interfaces;

/// <summary>
/// Abstraction for consuming messages from RabbitMQ.
/// </summary>
/// <remarks>
/// Implementations provide multiple consumption patterns:
/// - consume from an existing channel,
/// - create and consume using a new channel,
/// - or consume using a shared channel initialized by the consumer.
/// Implementations are expected to deserialize the raw message payload into <typeparamref name="T"/>
/// and invoke the provided <paramref name="handleMessage"/> callback for each message.
/// </remarks>
public interface IMessageBrokerConsumer : IMessageBrokerConsumerChannels
{
    /// <summary>
    /// Create a new channel and perform message consumption for messages of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The CLR type to which the message payload will be deserialized.</typeparam>
    /// <param name="queueName">Target queue name.</param>
    /// <param name="message">
    /// An instance of <typeparamref name="T"/> supplied to the method. (Implementation-specific; may be used as an initial seed/test message, or to publish before consuming — see implementation notes.)
    /// </param>
    /// <param name="exchangeName">Exchange to use for binding/publishing if required.</param>
    /// <param name="routingKey">Routing key for binding or publishing.</param>
    /// <param name="handleMessage">Async callback invoked for each deserialized message.</param>
    /// <param name="cancellationToken">Token used to stop consuming and to abort long-running operations.</param>
    /// <returns>A <see cref="Task"/> that completes when consumption stops or the operation is otherwise finished.</returns>
    /// <remarks>
    /// This method creates and owns a new channel for the duration of the operation.
    /// Assumption: the <paramref name="message"/> parameter may be implementation-specific. Confirm intended use with the implementation if needed.
    /// </remarks>
    Task ConsumeWithNewChannelAsync<T>(
        string queueName,
        T message,
        string exchangeName,
        string routingKey,
        Func<T, Task> handleMessage,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Consume messages of type <typeparamref name="T"/> using a shared channel maintained by the consumer implementation.
    /// </summary>
    /// <typeparam name="T">The CLR type to which the message payload will be deserialized.</typeparam>
    /// <param name="queueName">Queue to consume from.</param>
    /// <param name="exchangeName">Exchange to bind the queue to.</param>
    /// <param name="routingKey">Routing key to use when binding or filtering messages.</param>
    /// <param name="handleMessage">Async callback invoked for each deserialized message.</param>
    /// <param name="cancellationToken">Token used to stop consuming and to abort long-running operations.</param>
    /// <returns>A <see cref="Task"/> that completes when consumption stops or the operation finishes.</returns>
    /// <remarks>
    /// Before calling this method, the shared channel should be initialized with <see cref="InitializeSharedChannelAsync"/>.
    /// </remarks>
    Task ConsumeWithSharedChannelAsync<T>(
        string queueName,
        string exchangeName,
        string routingKey,
        Func<T, Task> handleMessage,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Asynchronously dispose the consumer and release resources (channels, connections, etc.).
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
    ValueTask DisposeAsync();

    /// <summary>
    /// Initialize a shared channel used by <see cref="ConsumeWithSharedChannelAsync{T}"/>.
    /// </summary>
    /// <param name="queueName">Queue to prepare on the shared channel.</param>
    /// <param name="exchangeName">Exchange to prepare on the shared channel.</param>
    /// <returns>A <see cref="Task"/> that completes when initialization finishes.</returns>
    Task InitializeSharedChannelAsync(string queueName, string exchangeName);
}
