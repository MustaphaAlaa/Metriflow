using System.Threading.Channels;
using System.Threading.Tasks;
using Metriflow.Domain;
using Metriflow.Domain.Entities.Enums;
using Metriflow.Domain.Entities.Workers;
using Metriflow.Domain.Interfaces;

namespace Metriflow.AggregationWorker.Interfaces.Correlation;

/// <summary>
/// Handles incoming analytic records received by the consumer.
/// Implementations are responsible for storing, correlating and triggering downstream processing.
/// </summary>
public interface IRawDataConsumerMessageHandler<T>
    where T : class, IAnalyticRecord
{
    // Task HandleIncomingGaRecordAsync( Channel<List<GARecord>> channel, CancellationToken stoppingToken);
    // Task HandleIncomingPsiRecordAsync( Channel<List<PSIRecord>> channel, CancellationToken stoppingToken);

    Task HandleIncomingAnalyticsRecordsAsync(
        Channel<List<T>> channel,
        CancellationToken stoppingToken
    );
}
