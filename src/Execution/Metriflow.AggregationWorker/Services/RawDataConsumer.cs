using Metriflow.AggregationWorker.Interfaces;
using Metriflow.Application.Entities;
using Metriflow.Application.Interfaces;
using Metriflow.Domain.Entities.Workers;
using Microsoft.Extensions.Options;

namespace Metriflow.AggregationWorker.Services;

/// <summary>
/// Top-level consumer that wires RabbitMQ consumer channels to message handling logic.
/// </summary>
public class RawDataConsumer : IRawDataConsumer
{
    private readonly ILogger<RawDataConsumer> _logger;
    private readonly RabbitMqSettings _rabbitMqSettings;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RawDataConsumer(
        ILogger<RawDataConsumer> logger,
        IOptions<RabbitMqSettings> options,
        IServiceScopeFactory serviceScopeFactory
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _rabbitMqSettings = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _serviceScopeFactory =
            serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    /// <inheritdoc />
    public async Task Consume(CancellationToken stoppingToken)
    {
        _logger.LogInformation("START CONSUMING............");
        var gaConsumerTasK = this.ConsumeGaRecords(stoppingToken);
        var psaConsumerTask = this.ConsumePsaRecords(stoppingToken);
        await Task.WhenAll(gaConsumerTasK, psaConsumerTask);
    }

    private async Task ConsumeGaRecords(CancellationToken stoppingToken)
    {
        _logger.LogInformation("START CONSUMING GA Record............");

        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<
            IRawAnalyticRecordConsumers<GARecord>
        >();

        await handler.Consume(
            _rabbitMqSettings.Queues.GA,
            _rabbitMqSettings.Queues.GA,
            stoppingToken
        );
    }

    private async Task ConsumePsaRecords(CancellationToken stoppingToken)
    {
        _logger.LogInformation("START CONSUMING PSA Records............");
        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<
            IRawAnalyticRecordConsumers<PSARecord>
        >();

        await handler.Consume(
            _rabbitMqSettings.Queues.PSA,
            _rabbitMqSettings.Queues.PSA,
            stoppingToken
        );
    }
}
 