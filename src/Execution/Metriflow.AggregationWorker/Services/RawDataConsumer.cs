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
        try
        {
            await this.ConsumeGeneric(
                queueName: _rabbitMqSettings.Queues.GA,
                routingKey: _rabbitMqSettings.Queues.GA,
                async (List<GARecord> ga) =>
                {
                    _logger.LogInformation($"{ga.Count} of {enTypesKey.GA} records are received.");

                    // Check if cancellation has been requested before attempting to create scope
                    stoppingToken.ThrowIfCancellationRequested();

                    try
                    {
                        if (ga.Count == 0) return;
                        using var scope = _serviceScopeFactory.CreateScope();
                        var handler = scope.ServiceProvider
                            .GetRequiredService<IRawDataConsumerMessageHandler<GARecord>>();
                        await handler.HandleIncomingRecordAsync(enTypesKey.GA, ga);
                        await Notify(enTypesKey.GA, ga.Count);
                    }
                    catch (ObjectDisposedException ex)
                    {
                        // Service provider is disposed, likely during shutdown
                        _logger.LogWarning(
                            "@@@@@@@@@@@@@@@Service provider was disposed while processing message. Application may be shutting down.");
                        // Throw OperationCanceledException to signal graceful shutdown and prevent retries
                        throw new OperationCanceledException(
                            "@@@@@@@@@@@@@@Service provider was disposed during processing", ex,
                            stoppingToken);
                    }
                },
                stoppingToken
            );
          
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "#@@@@@@Something went wrong");
        }
    }

    /// <summary>
    /// Start consuming PSI messages on a dedicated channel and forward them to the handler.
    /// </summary>
    private async Task ConsumePSI(CancellationToken stoppingToken)
    {
        try
        {
            await ConsumeGeneric(
                queueName: _rabbitMqSettings.Queues.PSI,
                routingKey: _rabbitMqSettings.Queues.PSI,
                async (List<PSIRecord> psi) =>
                {
                    _logger.LogInformation($"{psi.Count} of {enTypesKey.PSI} records are received.");
                    stoppingToken.ThrowIfCancellationRequested();

                    try
                    {
                        if (psi.Count == 0) return;

                        using var scope = _serviceScopeFactory.CreateScope();
                        var handler = scope.ServiceProvider
                            .GetRequiredService<IRawDataConsumerMessageHandler<PSIRecord>>();
                        await handler.HandleIncomingRecordAsync(enTypesKey.PSI, psi);

                        await Notify(enTypesKey.PSI, psi.Count);
                    }
                    catch (ObjectDisposedException ex)
                    {
                        // Service provider is disposed, likely during shutdown
                        _logger.LogWarning(
                            "Service provider was disposed while processing message. Application may be shutting down.");
                        // Throw OperationCanceledException to signal graceful shutdown and prevent retries
                        throw new OperationCanceledException("Service provider was disposed during processing", ex,
                            stoppingToken);
                    }
                },
                stoppingToken
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "#@@@@@@Something went wrong");
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

    public async Task Notify(enTypesKey type, int recordsCount)
    {
        try
        {
            await _producer.NotifyCompletedMessageAsync(new AggregationCompletedMessage
                {
                    CorrelationId = Guid.NewGuid(),
                    CompletedType = AggregationType.Records,
                    ProcessedCount = recordsCount,
                    CompletedAt = DateTime.UtcNow,
                },
                _rabbitMqSettings.Queues.Correlation,
                _rabbitMqSettings.Exchange);

            _logger.LogInformation($"@@@@@@@Event is sent for the {type} added records.");
            _logger.LogInformation($"@@@@@@@Finished Handling incoming {type}records.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "!!!@@@Something Went Wrong while notifying");
        }
    }
}