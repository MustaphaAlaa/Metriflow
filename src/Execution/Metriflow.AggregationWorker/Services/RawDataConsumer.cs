using System.Threading.Channels;
using Metriflow.AggregationWorker.Interfaces;
using Metriflow.AggregationWorker.Interfaces.Correlation;
using Metriflow.Application.Entities;
using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Workers;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities.Enums;
using Metriflow.Domain.Entities.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;


namespace Metriflow.AggregationWorker.Services;

/// <summary>
/// Top-level consumer that wires RabbitMQ consumer channels to message handling logic.
/// </summary>
public class RawDataConsumer : IRawDataConsumer
{
    private readonly ILogger<RawDataConsumer> _logger;
    private readonly IMessageBrokerConsumer _consumer;
    private readonly IProducer _producer;
    private readonly RabbitMqSettings _rabbitMqSettings;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RawDataConsumer(
        ILogger<RawDataConsumer> logger,
        IMessageBrokerConsumer consumer,
        IProducer producer,
        IOptions<RabbitMqSettings> options,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        _rabbitMqSettings = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    /// <inheritdoc />
    public async Task Consume(CancellationToken stoppingToken)
    {
        _logger.LogInformation("START CONSUMING............");
        var gaConsumerTasK = this.ConsumeGA(stoppingToken);
        var psiConsumerTask = this.ConsumePSI(stoppingToken);
        await Task.WhenAll(gaConsumerTasK, psiConsumerTask);
    }

    /// <summary>
    /// Start consuming GA messages on a dedicated channel and forward them to the handler.
    /// </summary>
    private async Task ConsumeGA(CancellationToken stoppingToken)
    {
        var channel = Channel.CreateBounded<List<GARecord>>(
            new BoundedChannelOptions(1000)
            {
                SingleWriter = true,
                SingleReader = false,
                FullMode = BoundedChannelFullMode.Wait,
            }
        );


        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider
            .GetRequiredService<IRawDataConsumerMessageHandler<GARecord>>();
        var workers = Task.Run(() =>
            handler.HandleIncomingGaRecordAsync(channel, stoppingToken)
        );

        try
        {
            await this.ConsumeGeneric(
                queueName: _rabbitMqSettings.Queues.GA,
                routingKey: _rabbitMqSettings.Queues.GA,
                async (List<GARecord> ga) =>
                {
                    _logger.LogInformation($"{ga.Count} of {enTypesKey.GA} records are received.");
                    _logger.LogInformation($"$###### GARecords Chunk Count:   ${ga.Count} ########");

                    stoppingToken.ThrowIfCancellationRequested();

                    try
                    {
                        if (ga.Count == 0) return;

                        await channel.Writer.WriteAsync(ga, stoppingToken);
                        _logger.LogInformation(
                            $"$###### GARecords Chunk Count:   ${ga.Count}. %%% Is wrote in Channel, Channel Count is ${channel.Reader.Count}%%%  ########");
                    }
                    catch (ObjectDisposedException ex)
                    {
                        // Service provider is disposed, likely during shutdown
                        _logger.LogWarning(ex,
                            "!!!!!!!!!!!!!!!!!!!!!! Service provider was disposed while processing message. Application may be shutting down. !!!!!!!!!!!");
                        // Throw OperationCanceledException to signal graceful shutdown and prevent retries
                        throw new OperationCanceledException(
                            "!!!!!!!!!!!!!!!!!!!!!! Service provider was disposed during processing. !!!!!!!!!", ex,
                            stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "!!!!!!!!!!!!!!!!!!!!!! An exception is thrown. Application may be shutting down.");
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
        finally
        {
            channel.Writer.Complete();
            await workers;
        }
    }

    /// <summary>
    /// Start consuming PSI messages on a dedicated channel and forward them to the handler.
    /// </summary>
    private async Task ConsumePSI(CancellationToken stoppingToken)
    {
        var channel = Channel.CreateBounded<List<PSIRecord>>(
            new BoundedChannelOptions(1000)
            {
                SingleWriter = true,
                SingleReader = false,
                FullMode = BoundedChannelFullMode.Wait,
            }
        );


        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider
            .GetRequiredService<IRawDataConsumerMessageHandler<PSIRecord>>();


        var workers = Task.Run(() => handler.HandleIncomingPsiRecordAsync(channel, stoppingToken));
        try
        {
            var now = DateTime.Now;
            await ConsumeGeneric(
                queueName: _rabbitMqSettings.Queues.PSI,
                routingKey: _rabbitMqSettings.Queues.PSI,
                async (List<PSIRecord> psi) =>
                {
                    _logger.LogInformation($"{psi.Count} of {enTypesKey.PSI} records are received.");


                    _logger.LogInformation($"$###### PSIRecords Chunk Count:   ${psi.Count}  ########");
                    stoppingToken.ThrowIfCancellationRequested();


                    try
                    {
                        if (psi.Count == 0) return;

                        await channel.Writer.WriteAsync(psi, stoppingToken);
                        _logger.LogInformation(
                            $"$###### PSIRecords Chunk Count:   ${psi.Count}. %%% Is wrote in Channel, Channel Count is ${channel.Reader.Count}%%% ########");
                    }
                    catch (ObjectDisposedException ex)
                    {
                        // Service provider is disposed, likely during shutdown
                        _logger.LogWarning(ex,
                            "!!!!!!!!!!!!!!!!!!!!!!!! Service provider was disposed while processing message. Application may be shutting down. !!!!!!!!!!!!");
                        // Throw OperationCanceledException to signal graceful shutdown and prevent retries
                        throw new OperationCanceledException(
                            "!!!!!!!!!!! Service provider was disposed during processing. !!!!!!!!!!!", ex,
                            stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "!!!!!!!!!!!!!!!!!!!!!! An exception is thrown. Application may be shutting down.");
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
        finally
        {
            channel.Writer.Complete();

            await workers;
        }
    }

    private async Task ConsumeGeneric<T>(
        string queueName,
        string routingKey,
        Func<T, Task> dlg,
        CancellationToken stoppingToken
    )
    {
        var analyticChannel = await _consumer.CreateNewChannelAsync();
        await _consumer.ConsumeFromChannelAsync<T>(
            analyticChannel,
            queueName,
            exchangeName: _rabbitMqSettings.Exchange,
            routingKey,
            dlg,
            stoppingToken
        );
    }
}