using RabbitMQ.Client;

namespace Metriflow.Producers.Interfaces;

/// <summary>
/// Interface defining the contract for producing analytics messages to a message broker.
/// </summary>
public interface IProducer
{
    Task ProducePSIAsync(PSIRecord[] data, IChannel channel);


    Task ProduceGAAsync(GARecord[] data, IChannel channel);
}