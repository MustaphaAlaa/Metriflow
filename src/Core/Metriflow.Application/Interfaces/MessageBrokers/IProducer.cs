using Metriflow.Application.Entities;
using Metriflow.Domain.Interfaces;

namespace Metriflow.Application.Interfaces.Workers;

public interface IProducer
{
    /// <summary>
    /// Publishes a batch of analytic records to a message broker.
    /// </summary>
    /// <typeparam name="T">
    /// The analytic record type. Must implement <see cref="IAnalyticRecord"/>.
    /// </typeparam>
    /// <param name="data">
    /// The batch of records to publish.
    /// </param>
    /// <param name="routingKey">
    /// The routing key used for message routing.
    /// </param>
    /// <param name="exchangeName">
    /// The target exchange name.
    /// </param>
    /// <returns>
    /// A task that completes when the publish operation finishes.
    /// </returns>
    public Task PublishAnalyticRecords<T>(
        IList<T> data,
        string routingKey,
        string exchangeName = "analytics.raw"
    )
        where T : IAnalyticRecord;

    Task NotifyCompletedMessageAsync(AggregationCompletedMessage message, string routingKey,
        string exchangeName);
}