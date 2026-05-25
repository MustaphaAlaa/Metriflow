
using System.Threading.Channels;
using Metriflow.Domain.Interfaces;

namespace Metriflow.AggregationWorker.Interfaces.Correlation;

/// <summary>
/// Handles incoming analytic records received by the consumer.
/// Implementations are responsible for storing, correlating and triggering downstream processing.
/// </summary>
public interface IRawDataConsumerMessageHandler<T>
    where T : class, IAnalyticRecord
{
    Task HandleIncomingAnalyticsRecordsAsync(
        Channel<List<T>> channel,
        CancellationToken stoppingToken
    );
}
