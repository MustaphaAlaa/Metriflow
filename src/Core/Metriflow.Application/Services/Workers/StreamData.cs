using System.Text.Json;
using System.Threading.Channels;
using Metriflow.Application.Interfaces.Workers;
using Metriflow.Domain.CustomAttributes;
using Microsoft.Extensions.DependencyInjection;

namespace Metriflow.Application.Services.Workers;

/// <summary>
/// Streams and processes large JSON files using a batched, parallel pipeline.
/// </summary>
/// <remarks>
/// <para>
/// This class implements a producer–consumer pipeline designed to process large JSON files
/// without loading them entirely into memory. Records are streamed from disk, batched,
/// and processed concurrently by multiple worker tasks.
/// </para>
///
/// <para>
/// The pipeline uses a bounded channel to apply Backpressure. When workers cannot keep up,
/// the producer waits instead of buffering unbounded data.
/// </para>
///
/// <para>
/// Two batch sizes are used intentionally:
/// <list type="bullet">
/// <item>
/// <description>
/// <c>batchSize</c>: Controls how many records are read from the file at once
/// (optimized for file I/O).
/// </description>
/// </item>
/// <item>
/// <description>
/// Internal publish batch size: Controls how many records a worker accumulates before
/// invoking the processing callback (optimized for message publishing).
/// </description>
/// </item>
/// </list>
/// </para>
/// </remarks>
[ServiceRegistration(ServiceLifetime.Scoped, typeof(IStreamData))]
public class StreamData : IStreamData
{
    /// <inheritdoc/>
    public async Task RunPipelineAsync<T>(
        string jsonFile,
        int batchSize,
        Func<List<T>, Task> onBatch
    )
    {
        var channel = Channel.CreateBounded<List<T>>(
            new BoundedChannelOptions(50)
            {
                SingleWriter = true,
                SingleReader = false,
                FullMode = BoundedChannelFullMode.Wait,
            }
        );

        var producerTask = ProducerTask(jsonFile, batchSize, channel);

        var workers = WorkersTask(channel, onBatch);

        await Task.WhenAll(workers.Prepend(producerTask));
    }

    /// <summary>
    /// Reads records from the JSON file, batches them, and writes them into the channel.
    /// </summary>
    /// <typeparam name="T">The type of records being produced.</typeparam>
    /// <param name="jsonFile">Path to the JSON file.</param>
    /// <param name="batchSize">Number of records per batch written to the channel.</param>
    /// <param name="channel">The bounded channel used to pass data to worker tasks.</param>
    /// <returns>
    /// A task representing the lifetime of the producer.
    /// </returns>
    /// <remarks>
    /// This method runs as a single producer. When the channel is full, writes will wait,
    /// applying backpressure. The channel is completed once all data has been written.
    /// </remarks>

    private Task ProducerTask<T>(string jsonFile, int batchSize, Channel<List<T>> channel)
    {
        var producer = Task.Run(async () =>
        {
            try
            {
                var stream = StreamDataFromJsonAsync<T>(jsonFile);

                await foreach (var batch in BatchStreamAsync(stream, batchSize))
                {
                    await channel.Writer.WriteAsync(batch);
                }
            }
            finally
            {
                channel.Writer.Complete();
            }
        });
        return producer;
    }


    /// <summary>
    /// Streams records from a JSON file using asynchronous deserialization.
    /// </summary>
    /// <typeparam name="T">The type of records to deserialize.</typeparam>
    /// <param name="filePath">Path to the JSON file.</param>
    /// <returns>
    /// An asynchronous stream of deserialized records.
    /// </returns>
    /// <remarks>
    /// Records are read incrementally from disk to avoid loading the entire file into memory.
    /// Null records are skipped.
    /// </remarks>
    private async IAsyncEnumerable<T> StreamDataFromJsonAsync<T>(string filePath)
    {
        await using var fs = File.OpenRead(filePath);
        await foreach (var record in JsonSerializer.DeserializeAsyncEnumerable<T>(fs))
        {
            if (record != null)
            {
                yield return record;
            }
        }
    }

    /// <summary>
    /// Groups streamed records into fixed-size batches.
    /// </summary>
    /// <typeparam name="T">The type of records being batched.</typeparam>
    /// <param name="source">The source asynchronous stream.</param>
    /// <param name="size">The maximum number of records per batch.</param>
    /// <returns>
    /// An asynchronous stream of record batches.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="size"/> is less than or equal to zero.
    /// </exception>
    /// <remarks>
    /// This method is used to control how many records are emitted downstream at once,
    /// typically to optimize file I/O and channel throughput.
    /// </remarks>
    private static async IAsyncEnumerable<List<T>> BatchStreamAsync<T>(
        IAsyncEnumerable<T> source,
        int size
    )
    {
        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size));

        var buffer = new List<T>();
        await foreach (var item in source)
        {
            buffer.Add(item);
            if (buffer.Count >= size)
            {
                yield return buffer;
                buffer = new();
            }
        }

        if (buffer.Count > 0)
        {
            yield return buffer;
        }
    }

    /// <summary>
    /// Creates and starts worker tasks that consume batches from the channel and process them.
    /// </summary>
    /// <typeparam name="T">The type of records being processed.</typeparam>
    /// <param name="channel">The channel from which batches are consumed.</param>
    /// <param name="onBatch">
    /// Callback invoked when a publish-sized batch is ready.
    /// </param>
    /// <returns>
    /// A list of running worker tasks.
    /// </returns>
    /// <remarks>
    /// Each worker accumulates records into a publish-sized batch before invoking the callback.
    /// Workers run concurrently and drain the channel until it is completed.
    /// </remarks>

    private List<Task> WorkersTask<T>(Channel<List<T>> channel, Func<List<T>, Task> onBatch)
    {
        const int publishBatchSize = 5000;
        var workers = Enumerable
            .Range(0, 4)
            .Select(_ =>
                Task.Run(async () =>
                {
                    var lst = new List<T>();

                    await foreach (var batch in channel.Reader.ReadAllAsync())
                    {
                        foreach (var obj in batch)
                        {
                            lst.Add(obj);
                            if (lst.Count >= publishBatchSize)
                            {
                                await onBatch(lst);
                                lst = new();
                            }
                        }
                    }

                    if (lst.Count > 0)
                    {
                        await onBatch(lst);
                    }
                })
            )
            .ToList();
        return workers;
    }
}
