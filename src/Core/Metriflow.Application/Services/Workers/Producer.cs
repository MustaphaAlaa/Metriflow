using Metriflow.Application.Interfaces.Workers;
using Metriflow.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.Services.Workers;

/// <summary>
/// Publishes analytic record batches to a message broker.
/// </summary>
/// <remarks>
/// Acts as an adapter between the streaming pipeline and the message broker,
/// providing logging and isolating broker-specific concerns from application logic.
/// </remarks>
public class Producer : IProducer
{
    private readonly IMessageBrokerProducer _messageBrokerProducer;

    private ILogger<Producer> _logger;

    /// <summary>
    /// Initializes a new instance of the Producer class.
    /// </summary>
    /// <param name="messageBrokerProducer">The message broker producer implementation to use for publishing.</param>
    /// <param name="logger">Logger for recording publish operations and diagnostics.</param>
    public Producer(IMessageBrokerProducer messageBrokerProducer, ILogger<Producer> logger)
    {
        _logger = logger;
        _messageBrokerProducer = messageBrokerProducer;
    }

    /// <summary>
    /// Publishes a batch of analytic records to the message broker and logs the operation.
    /// </summary>
    /// <typeparam name="T">The type of analytic record, must implement IAnalyticRecord.</typeparam>
    /// <param name="data">The list of analytic records to publish. Assumed to be non-empty and contain records with Date properties.</param>
    /// <param name="routingKey">The routing key for message routing (e.g., "analytics.raw.GA").</param>
    /// <param name="exchangeName">The exchange name to publish to (e.g., "analytics.raw.GA").</param>
    /// <exception cref="IndexOutOfRangeException">Thrown if data is empty when accessing data[0] for logging.</exception>
    /// <exception cref="Exception">May throw exceptions from the underlying message broker if publishing fails.</exception>
    public async Task PublishAnalyticRecords<T>(
        IList<T> data,
        string routingKey,
        string exchangeName
    )
        where T : IAnalyticRecord
    {
        await _messageBrokerProducer.PublishAsync(data, exchangeName, routingKey, true);
        _logger.LogInformation("Running on Thread: {thread}", Thread.CurrentThread.ManagedThreadId);

        var recordCount = data.Count;
        var fromDate = data.Count > 0 ? data[0].Date : 0;
        var toDate = data.Count > 0 ? data[data.Count - 1].Date : 0;

        _logger.LogInformation(
            "On routing key: {routingKey}, {type} → numbers of records has been published {record}, From: {from}, To: {to}",
            routingKey,
            typeof(T).Name,
            recordCount,
            fromDate,
            toDate
        );
    }

    
}
