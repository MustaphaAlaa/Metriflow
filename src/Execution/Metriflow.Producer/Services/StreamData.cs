using System.Text.Json;
using System.Threading.Channels;
using Metriflow.Correlation.Worker;
using Metriflow.Producers.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Metriflow.Producers.Implementation;

/// <summary>
/// Implements the data seeding functionality for analytics records.
/// Loads mock data from JSON files for both GA and PSI records.
/// </summary>
public class StreamData : IStreamData
{
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the SeedData class and loads mock data.
    /// </summary>
    /// <param name="environment">The host environment for accessing content root path.</param>
    public StreamData(IHostEnvironment environment)
    {
        _environment = environment;
    }


    private async IAsyncEnumerable<T> StreamDataFromJSONAsync<T>(string filename)
    {
        var filePath = Path.Combine(_environment.ContentRootPath, "data", filename);

        await using var fs = File.OpenRead(filePath);

        await foreach (var record in JsonSerializer.DeserializeAsyncEnumerable<T>(fs))
        {
            if (record != null) yield return record;
        }
    }

    // private static async IAsyncEnumerable<List<T>> ToBatches<T>(
    //     IAsyncEnumerable<T> source, int batchSize)
    // {
    //     var batch = new List<T>(batchSize);
    //
    //     await foreach (var item in source)
    //     {
    //         batch.Add(item);
    //         if (batch.Count == batchSize)
    //         {
    //             yield return batch;
    //             batch = new List<T>(batchSize);
    //         }
    //     }
    //
    //     if (batch.Count > 0)
    //         yield return batch;
    // }

    /// <inheritdoc/>
    // public async Task SeedingData()
    // {
    //     GARecords = GetDataFromJSONAsync<GARecord>("GA-mock.json");
    //     PSIRecords = GetDataFromJSONAsync<PSIRecord>("PSI-mock.json");
    // }
    // public async Task RunPipelineAsync<T>(
    //     string filePath,
    //     int batchSize,
    //     int workers,
    //     Func<List<T>, Task> produceAsync,
    //     CancellationToken token = default)
    // {
    //     // bounded buffer avoids RAM explosion
    //     var channel = Channel.CreateBounded<List<T>>(capacity: workers * 2);
    //
    //     // --- READER TASK ---
    //     var readerTask = Task.Run(async () =>
    //     {
    //         try
    //         {
    //             await foreach (var batch in ToBatches(StreamDataFromJSONAsync<T>(filePath), batchSize))
    //             {
    //                 await channel.Writer.WriteAsync(batch, token);
    //             }
    //         }
    //         finally
    //         {
    //             channel.Writer.Complete();
    //         }
    //     });
    //
    //     // --- WORKERS ---
    //
    //     var workerTasks = Workers(channel, workers, produceAsync, token);
    //     // wait all
    //     await readerTask;
    //     await Task.WhenAll(workerTasks);
    // }
    //
    // private async Task<List<Task>> Workers<T>(Channel<List<T>> channel, int workers, Func<List<T>, Task> produceAsync, 
    //     , CancellationToken token = default)
    // {
    //     var workerTasks = Enumerable.Range(0, workers)
    //         .Select(_ => Task.Run(async () =>
    //         {
    //             await foreach (var batch in channel.Reader.ReadAllAsync(token))
    //             {
    //                 await produceAsync(batch); // RabbitMQ publishing
    //             }
    //         }))
    //         .ToList();
    //     return workerTasks;
    // }

    // // ----------- PARSER -----------
    // private async IAsyncEnumerable<T> StreamJsonAsync<T>(string filename)
    // {
    //     var path = Path.Combine(_environment.ContentRootPath, "data", filename);
    //
    //     await using var fs = File.OpenRead(path);
    //
    //     await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<T>(fs))
    //     {
    //         if (item != null)
    //             yield return item;
    //     }
    // }

    // ----------- BATCHER -----------
    private static async IAsyncEnumerable<List<T>> BatchAsync<T>(
        IAsyncEnumerable<T> source,
        int size)
    {
        var buffer = new List<T>(size);

        await foreach (var item in source)
        {
            buffer.Add(item);

            if (buffer.Count == size)
            {
                yield return buffer;
                buffer = new List<T>(size);
            }
        }

        if (buffer.Count > 0)
            yield return buffer;
    }

    // ----------- PIPELINE RUNNER -----------
    public async Task RunPipelineAsync<T>(
        string jsonFile,
        int batchSize,
        Func<List<T>, Task> onBatch)
    {
        var stream = StreamDataFromJSONAsync<T>(jsonFile);

        await foreach (var batch in BatchAsync(stream, batchSize))
        {
            await onBatch(batch);
        }
    }
}