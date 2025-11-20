using RabbitMQ.Client;

namespace Metriflow.Producers.Interfaces;

/// <summary>
/// Interface defining the contract for accessing seed data for analytics records.
/// </summary>
public interface IStreamData
{
    Task RunPipelineAsync<T>(
        string jsonFile,
        int batchSize,
        Func<List<T>, IChannel, Task> onBatch);
}