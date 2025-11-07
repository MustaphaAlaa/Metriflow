using System.Threading;
using System.Threading.Tasks;

namespace Metriflow.Correlation.Worker.Interfaces;

/// <summary>
/// A long-running consumer that listens for incoming messages and processes them.
/// </summary>
public interface ICorrelationConsumer
{
    /// <summary>
    /// Starts consuming messages until the provided <see cref="CancellationToken"/> is signaled.
    /// </summary>
    /// <param name="stoppingToken">Token used to request graceful shutdown of the consumer.</param>
    Task Consume(CancellationToken stoppingToken);
}
