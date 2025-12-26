namespace Metriflow.Application.Interfaces.Workers;

/// <summary>
/// Defines a contract for streaming and processing large JSON files in batches.
/// </summary>
/// <remarks>
/// <para>
/// Implementations are expected to stream records incrementally, apply batching,
/// enforce backpressure, and process data asynchronously without loading the entire
/// file into memory.
/// </para>
/// </remarks>
public interface IStreamData
{
    /// <summary>
    /// Processes a JSON file by streaming records in batches and invoking the provided callback for each batch.
    /// </summary>
    /// <typeparam name="T">The type of records to deserialize from the JSON file.</typeparam>
    /// <param name="jsonFile">Path to the JSON file to process.</param>
    /// <param name="batchSize">Number of records to read from the file and place into the channel per batch. Used for optimizing file I/O.</param>
    /// <param name="onBatch">Callback function invoked when a batch of records is ready for processing. The batch size passed to this callback is determined by the internal publish batch size (1200), not the batchSize parameter.</param>
    /// <returns>A task that completes when all records from the file have been processed.</returns>
    Task RunPipelineAsync<T>(string jsonFile, int batchSize, Func<List<T>, Task> onBatch);
}
