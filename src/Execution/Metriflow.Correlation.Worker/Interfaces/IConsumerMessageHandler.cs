namespace Metriflow.Correlation.Worker.Interfaces;

/// <summary>
/// Defines a contract for handling messages delivered by consumers.
///
/// Implementations receive a typed object and additional metadata (raw message text,
/// a Redis hash key and field name) and are expected to process the message accordingly.
/// </summary>
public interface IConsumerMessageHandler
{
    /// <summary>
    /// Handle an incoming message.
    /// </summary>
    /// <typeparam name="T">The CLR type the message payload has been deserialized to.</typeparam>
    /// <param name="obj">The deserialized message object.</param>
    /// <param name="message">A human-readable or raw representation of the message (for logging or debugging).</param>
    /// <param name="hashKey">Redis hash key used for storing or correlating this message (e.g., "ga" or "psi").</param>
    /// <param name="fieldName">Field name inside the Redis hash used for this message (e.g., "ga:2025-01-01|/home").</param>
    /// <returns>A task that completes when message handling has finished.</returns>
    Task MessageHandler<T>(T obj, string message, string hashKey, string fieldName);
}
