using System.Threading.Channels;
using Metriflow.AggregationWorker.Interfaces;
using Metriflow.AggregationWorker.Interfaces.Correlation;
using Metriflow.Application.Entities;
using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Workers;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities.Enums;
using Metriflow.Domain.Entities.Workers;
using Metriflow.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace Metriflow.AggregationWorker.Services;

/// <summary>
/// Top-level consumer that wires RabbitMQ consumer channels to message handling logic.
/// </summary>
public class RawDataConsumer : IRawDataConsumer
{
    private readonly ILogger<RawDataConsumer> _logger;
    private readonly IMessageBrokerConsumerChannels _consumer;
    private readonly RabbitMqSettings _rabbitMqSettings;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    const int workersCount = 4;

    // prefetchCount * workersCount * 3
    const int boundChannelSize = 30 * workersCount * 3;

    public RawDataConsumer(
        ILogger<RawDataConsumer> logger,
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

    /// <inheritdoc />
    public async Task Consume(CancellationToken stoppingToken)
    {
        _logger.LogInformation("START CONSUMING............");

        var gaConsumerTasK = this.ConsumeGARecords(stoppingToken);
        var psiConsumerTask = this.ConsumePSI(stoppingToken);
        await Task.WhenAll(gaConsumerTasK, psiConsumerTask);
    }

    /// <summary>
    /// Start consuming GA messages on a dedicated channel and forward them to the handler.
    /// </summary>
    private async Task ConsumeGARecords(CancellationToken stoppingToken)
    {
        var channel = Channel.CreateBounded<List<GARecord>>(
            new BoundedChannelOptions(boundChannelSize)
            {
                SingleWriter = false,
                SingleReader = true,
                FullMode = BoundedChannelFullMode.Wait,
            }
        );

        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<
            IRawDataConsumerMessageHandler<GARecord>
        >();

        var workers = Task.Run(() =>
            handler.HandleIncomingAnalyticsRecordsAsync(channel, stoppingToken)
        );

        var analyticChannel = await _consumer.CreateNewChannelAsync();

        var gaConsumers = Enumerable
            .Range(0, workersCount)
            .Select(workerId =>
                Task.Run(async () =>
                {
                    try
                    {
                        await _consumer.ConsumeFromChannelAsync(
                            channel: analyticChannel,
                            queueName: _rabbitMqSettings.Queues.GA,
                            exchangeName: _rabbitMqSettings.Exchange,
                            routingKey: _rabbitMqSettings.Queues.GA,
                            async (List<GARecord> ga) =>
                            {
                                _logger.LogInformation(
                                    $"{ga.Count} of {enTypesKey.GA} records are received."
                                );
                                _logger.LogInformation(
                                    $"$###### GARecords Chunk Count:   ${ga.Count} ########"
                                );

                                stoppingToken.ThrowIfCancellationRequested();

                                try
                                {
                                    if (ga.Count == 0)
                                        return;

                                    await channel.Writer.WriteAsync(ga, stoppingToken);
                                    _logger.LogInformation(
                                        $"$###### GARecords Chunk Count:   ${ga.Count}. %%% Is wrote in Channel, Channel Count is ${channel.Reader.Count}%%%  ########"
                                    );
                                }
                                catch (ObjectDisposedException ex)
                                {
                                    // Service provider is disposed, likely during shutdown
                                    _logger.LogWarning(
                                        ex,
                                        "!!!!!!!!!!!!!!!!!!!!!! Service provider was disposed while processing message. Application may be shutting down. !!!!!!!!!!!"
                                    );
                                    // Throw OperationCanceledException to signal graceful shutdown and prevent retries
                                    throw new OperationCanceledException(
                                        "!!!!!!!!!!!!!!!!!!!!!! Service provider was disposed during processing. !!!!!!!!!",
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

        await Task.WhenAll(gaConsumers);
        channel.Writer.Complete();
        await workers;
    }

    /// <summary>
    /// Start consuming PSI messages on a dedicated channel and forward them to the handler.
    /// </summary>
    private async Task ConsumePSI(CancellationToken stoppingToken)
    {
        var channel = Channel.CreateBounded<List<PSIRecord>>(
            new BoundedChannelOptions(boundChannelSize)
            {
                SingleWriter = false,
                SingleReader = true,
                FullMode = BoundedChannelFullMode.Wait,
            }
        );

        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<
            IRawDataConsumerMessageHandler<PSIRecord>
        >();

        var workers = Task.Run(() =>
            handler.HandleIncomingAnalyticsRecordsAsync(channel, stoppingToken)
        );
        var analyticChannel = await _consumer.CreateNewChannelAsync();

        var psiConsumers = Enumerable
            .Range(0, workersCount)
            .Select(workerId =>
                Task.Run(async () =>
                {
                    try
                    {
                        var now = DateTime.Now;

                        await _consumer.ConsumeFromChannelAsync(
                            channel: analyticChannel,
                            queueName: _rabbitMqSettings.Queues.PSI,
                            exchangeName: _rabbitMqSettings.Exchange,
                            routingKey: _rabbitMqSettings.Queues.PSI,
                            async (List<PSIRecord> psi) =>
                            {
                                _logger.LogInformation(
                                    $"{psi.Count} of {enTypesKey.PSI} records are received."
                                );

                                _logger.LogInformation(
                                    $"$###### PSIRecords Chunk Count:   ${psi.Count}  ########"
                                );
                                stoppingToken.ThrowIfCancellationRequested();

                                try
                                {
                                    if (psi.Count == 0)
                                        return;

                                    await channel.Writer.WriteAsync(psi, stoppingToken);
                                    _logger.LogInformation(
                                        $"$###### PSIRecords Chunk Count:   ${psi.Count}. %%% Is wrote in Channel, Channel Count is ${channel.Reader.Count}%%% ########"
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

        await Task.WhenAll(psiConsumers);
        channel.Writer.Complete();
        await workers;
    }
}
