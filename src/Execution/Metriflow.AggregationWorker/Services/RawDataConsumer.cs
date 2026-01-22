using Metriflow.AggregationWorker.Interfaces;
using Metriflow.AggregationWorker.Interfaces.Correlation;
using Metriflow.Application.Entities;
using Metriflow.Application.Interfaces;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities.Enums;
using Metriflow.Domain.Entities.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;


namespace Metriflow.AggregationWorker.Services;

/// <summary>
/// Top-level consumer that wires RabbitMQ consumer channels to message handling logic.
/// </summary>
// [ServiceRegistration(ServiceLifetime.Singleton, typeof(IRawDataConsumer))]
public class RawDataConsumer : IRawDataConsumer
{
    private readonly ILogger<RawDataConsumer> _logger;
    private readonly IMessageBrokerConsumer _consumer;
    private readonly RabbitMqSettings _settings;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RawDataConsumer(
        ILogger<RawDataConsumer> logger,
        IMessageBrokerConsumer consumer,
        IOptions<RabbitMqSettings> options,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
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
        _logger.LogInformation(@$"START CONSUMING
                                -- From Queue: {enTypesKey.GA}-Queue
                                -- From Exchange: analytics.raw.{enTypesKey.GA}");


        await this.ConsumeGeneric(
            queueName: _settings.Queues.GA,
            routingKey: _settings.Queues.GA,
            async (List<GARecord> ga) =>
            {
                _logger.LogInformation("############We are here");

                _logger.LogInformation("##########THere are records are received.");
                _logger.LogInformation($"{ga.Count} of {enTypesKey.GA} records are received.");

                // Check if cancellation has been requested before attempting to create scope
                stoppingToken.ThrowIfCancellationRequested();

                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var handler = scope.ServiceProvider
                        .GetRequiredService<IConsumerMessageHandler<GARecord>>();
                    await handler.HandleIncomingRecordAsync(enTypesKey.GA, ga);

                }
                catch (ObjectDisposedException ex)
                {
                    // Service provider is disposed, likely during shutdown
                    _logger.LogWarning("Service provider was disposed while processing message. Application may be shutting down.");
                    // Throw OperationCanceledException to signal graceful shutdown and prevent retries
                    throw new OperationCanceledException("Service provider was disposed during processing", ex, stoppingToken);
                }
            },
            stoppingToken
        );
    }

    /// <summary>
    /// Start consuming PSI messages on a dedicated channel and forward them to the handler.
    /// </summary>
    private async Task ConsumePSI(CancellationToken stoppingToken)
    {
        await ConsumeGeneric(
            queueName: _settings.Queues.PSI,
            routingKey: _settings.Queues.PSI,
            async (List<PSIRecord> psi) =>
            {
                _logger.LogInformation("We are here");
                _logger.LogInformation($"{psi.Count} of {enTypesKey.PSI} records are received.");

                // Check if cancellation has been requested before attempting to create scope
                stoppingToken.ThrowIfCancellationRequested();

                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var handler = scope.ServiceProvider
                        .GetRequiredService<IConsumerMessageHandler<PSIRecord>>();
                    await handler.HandleIncomingRecordAsync(enTypesKey.PSI, psi);


                }
                catch (ObjectDisposedException ex)
                {
                    // Service provider is disposed, likely during shutdown
                    _logger.LogWarning("Service provider was disposed while processing message. Application may be shutting down.");
                    // Throw OperationCanceledException to signal graceful shutdown and prevent retries
                    throw new OperationCanceledException("Service provider was disposed during processing", ex, stoppingToken);
                }
            },
            stoppingToken
        );
    }

    private async Task ConsumeGeneric<T>(
        string queueName,
        string routingKey,
        Func<T, Task> dlg,
        CancellationToken stoppingToken
    )
    {
        _logger.LogInformation("#########Inside ConsumeGeneric");
        var analyticChannel = await _consumer.CreateNewChannelAsync();
        _logger.LogInformation(queueName);
        _logger.LogInformation(routingKey);
        await _consumer.ConsumeFromChannelAsync<T>(
            analyticChannel,
            queueName,
            exchangeName: _settings.Exchange,
            routingKey,
            dlg,
            stoppingToken
        );
    }
}