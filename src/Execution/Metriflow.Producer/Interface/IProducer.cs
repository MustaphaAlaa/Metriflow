namespace Metriflow.Producers.Interfaces;

/// <summary>
/// Interface defining the contract for producing analytics messages to a message broker.
/// </summary>
public interface IProducer
{
    /// <summary>
    /// Produces both Google Analytics and Page Speed Insights data to the message broker.
    /// </summary>
    /// <param name="gaData">List of Google Analytics records to be published.</param>
    /// <param name="paData">List of Page Speed Insights records to be published.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Produce(List<GARecord> gaData, List<PSIRecord> paData);
}
