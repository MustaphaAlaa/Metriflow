using System.Text.Json;
using System.Threading.Channels;
using Metriflow.Application.interfaces;
using Metriflow.Correlation.Worker;
using Metriflow.Producers.Interfaces;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace Metriflow.Producers.Implementation;

/// <summary>
/// Implements the data seeding functionality for analytics records.
/// Loads mock data from JSON files for both GA and PSI records.
/// </summary>
public class StreamData : IStreamData
{
    private readonly IHostEnvironment _environment;
    private readonly IRabbitMQProducer _rabbitMQProducer;

    public StreamData(IHostEnvironment environment, IRabbitMQProducer rabbitMqProducer)
    {
        _environment = environment;
        _rabbitMQProducer = rabbitMqProducer;
    }

    private async IAsyncEnumerable<T> StreamDataFromJSONAsync<T>(string filename)
    {
        var filePath = Path.Combine(_environment.ContentRootPath, "data", filename);

        await using var fs = File.OpenRead(filePath);

        await foreach (var record in JsonSerializer.DeserializeAsyncEnumerable<T>(fs))
        {
            if (record != null)
                yield return record;
        }
    }

    private static async IAsyncEnumerable<List<T>> BatchAsync<T>(
        IAsyncEnumerable<T> source,
        int size
    )
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

    public async Task RunPipelineAsync<T>(
        string jsonFile,
        int batchSize,
        Func<List<T>, IChannel, Task> onBatch
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

    private List<Task> WorkersTask<T>(
        Channel<List<T>> channel,
        Func<List<T>, IChannel, Task> onBatch
    )
    {
        const int patchPublishSize = 6000;
        var workers = Enumerable
            .Range(0, 4) // 4 workers
            .Select(_ =>
                Task.Run(async () =>
                {
                    var rabbitMQChannel = await _rabbitMQProducer.CreateNewChannelAsync(
                        "analytics.raw"
                    );
                    var accumulator = new List<T>(patchPublishSize);
                    await foreach (var batch in channel.Reader.ReadAllAsync())
                    {
                        accumulator.AddRange(batch);
                        if (accumulator.Count >= patchPublishSize)
                        {
                            await onBatch(
                                accumulator.Take(patchPublishSize).ToList(),
                                rabbitMQChannel
                            );
                            accumulator.RemoveRange(0, patchPublishSize);
                        }
                    }

                    if (accumulator.Count > 0)
                    {
                        await onBatch(accumulator, rabbitMQChannel);
                    }
                })
            )
            .ToList();
        return workers;
    }

    private Task ProducerTask<T>(string jsonFile, int batchSize, Channel<List<T>> channel)
    {
        var Producer = Task.Run(async () =>
        {
            try
            {
                var stream = StreamDataFromJSONAsync<T>(jsonFile);

                await foreach (var batch in BatchAsync(stream, batchSize))
                {
                    await channel.Writer.WriteAsync(batch);
                }
            }
            finally
            {
                channel.Writer.Complete();
            }
        });
        return Producer;
    }
}
