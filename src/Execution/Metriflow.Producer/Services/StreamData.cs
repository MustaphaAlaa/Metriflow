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

    private static async IAsyncEnumerable<T[]> BatchAsync<T>(
        IAsyncEnumerable<T> source,
        int size
    )
    {
        var buffer = new T[size];
        int count = 0;

        await foreach (var item in source)
        {
            buffer[count++] = item;

            if (buffer.Length == size)
            {
                yield return buffer;
                buffer = new T[size];
            }
        }

        if (buffer.Length > 0)
            yield return buffer;
    }

    public async Task RunPipelineAsync<T>(
        string jsonFile,
        int batchSize,
        Func<T[], IChannel, Task> onBatch
    )
    {
        var channel = Channel.CreateBounded<T[]>(
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
        Channel<T[]> channel,
        Func<T[], IChannel, Task> onBatch
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


                    var arr = new T[patchPublishSize];
                    short count = 0;
                    await foreach (var batch in channel.Reader.ReadAllAsync())
                    {
                        foreach (var obj in batch)
                        {
                            arr[count++] = obj;
                            if (arr.Length == patchPublishSize)
                            {
                                await onBatch(
                                    arr,
                                    rabbitMQChannel
                                );
                                count = 0;
                            }
                        }
                    }


                    if (count > 0)
                    {
                        await onBatch(
                            arr,
                            rabbitMQChannel
                        );
                    }
                })
            )
            .ToList();
        return workers;
    }

    private Task ProducerTask<T>(string jsonFile, int batchSize, Channel<T[]> channel)
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