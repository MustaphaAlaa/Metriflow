using System.Threading.Tasks;
using Metriflow.Domain;
using Metriflow.Domain.Interfaces;

namespace Metriflow.Correlation.Worker.Interfaces;

/// <summary>
/// Handles incoming analytic records received by the consumer.
/// Implementations are responsible for storing, correlating and triggering downstream processing.
/// </summary>
public interface IConsumerMessageHandler
{
    /// <summary>
    /// Handle an incoming analytic record of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The record type, must implement <see cref="IAnalyticRecord"/>.</typeparam>
    /// <param name="type">A short type identifier (for example, "ga" or "psi").</param>
    /// <param name="record">The record instance to be handled.</param>
    Task HandleIncomingRecordAsync<T>(string type, IList<T> record)
        where T : IAnalyticRecord;
}
