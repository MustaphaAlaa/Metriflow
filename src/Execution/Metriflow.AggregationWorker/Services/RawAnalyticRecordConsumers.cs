using System.Threading.Channels;
using Metriflow.AggregationWorker.Interfaces;
using Metriflow.AggregationWorker.Interfaces.Correlation;
using Metriflow.Application.Entities;
using Metriflow.Application.Interfaces;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace Metriflow.AggregationWorker.Services;

[ServiceRegistration(ServiceLifetime.Scoped, typeof(IRawAnalyticRecordConsumers<>))]
public class RawAnalyticRecordConsumers<T> : IRawAnalyticRecordConsumers<T>
    where T : class, IAnalyticRecord
{
    private readonly ILogger<RawAnalyticRecordConsumers<T>> _logger;
    private readonly IMessageBrokerConsumerChannels _consumer;
    private readonly RabbitMqSettings _rabbitMqSettings;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    const int workersCount = 4;

    // prefetchCount * workersCount * 3
    const int boundChannelSize = 30 * workersCount * 3;

    public RawAnalyticRecordConsumers(
        ILogger<RawAnalyticRecordConsumers<T>> logger,
        IMessageBrokerConsumer consumer,
        IOptions<RabbitMqSettings> options,
        IServiceScopeFactory serviceScopeFactory
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        _rabbitMqSettings = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _serviceScopeFactory =
            serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    /// <summary>
    /// Start consuming PSI messages on a dedicated channel and forward them to the handler.
    /// </summary>
    public async Task Consume(string queueName, string routingKey, CancellationToken stoppingToken)
    {
        var channel = Channel.CreateBounded<List<T>>(
            new BoundedChannelOptions(boundChannelSize)
            {
                SingleWriter = false,
                SingleReader = true,
                FullMode = BoundedChannelFullMode.Wait,
            }
        );

        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IRawDataConsumerMessageHandler<T>>();

        var workers = Task.Run(() =>
            handler.HandleIncomingAnalyticsRecordsAsync(channel, stoppingToken)
        );
        var analyticChannel = await _consumer.CreateNewChannelAsync();

        var consumers = Enumerable
            .Range(0, workersCount)
            .Select(workerId =>
                Task.Run(async () =>
                {
                    try
                    {
                        var now = DateTime.Now;

                        await _consumer.ConsumeFromChannelAsync(
                            channel: analyticChannel,
                            queueName,
                            exchangeName: _rabbitMqSettings.Exchange,
                            routingKey,
                            async (List<T> recordsLst) =>
                            {
                                _logger.LogInformation(
                                    $"{recordsLst.Count} of ${nameof(T)} records are received."
                                );

                                _logger.LogInformation(
                                    $"$###### ${nameof(T)} Chunk Count:   ${recordsLst.Count}  ########"
                                );
                                stoppingToken.ThrowIfCancellationRequested();

                                try
                                {
                                    if (recordsLst.Count == 0)
                                        return;

                                    await channel.Writer.WriteAsync(recordsLst, stoppingToken);
                                    _logger.LogInformation(
                                        $"$###### ${nameof(T)} Chunk Count:   ${recordsLst.Count}. %%% Is wrote in Channel, Channel Count is ${channel.Reader.Count}%%% ########"
                                    );
                                }
                                catch (ObjectDisposedException ex)
                                {
                                    // Service provider is disposed, likely during shutdown
                                    _logger.LogWarning(
                                        ex,
                                        "!!!!!!!!!!!!!!!!!!!!!!!! Service provider was disposed while processing message. Application may be shutting down. !!!!!!!!!!!!"
                                    );
                                    // Throw OperationCanceledException to signal graceful shutdown and prevent retries
                                    throw new OperationCanceledException(
                                        "!!!!!!!!!!! Service provider was disposed during processing. !!!!!!!!!!!",
                                        ex,
                                        stoppingToken
                                    );
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(
                                        ex,
                                        "!!!!!!!!!!!!!!!!!!!!!! An exception is thrown. Application may be shutting down."
                                    );
                                    throw;
                                }
                            },
                            stoppingToken
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "#@@@@@@Something went wrong");
                    }
                })
            );

        await Task.WhenAll(consumers);
        channel.Writer.Complete();
        await workers;
    }
}
