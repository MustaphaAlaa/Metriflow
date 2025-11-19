namespace Metriflow.Producers.Interfaces;

/// <summary>
/// Interface defining the contract for producing analytics messages to a message broker.
/// </summary>
public interface IProducer
{
    Task ProducePSIAsync(IList<PSIRecord> data);


    Task ProduceGAAsync(IList<GARecord> data);
}