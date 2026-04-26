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
        var gaConsumerTasK = this.ConsumeGARecords(stoppingToken);
        var psiConsumerTask = this.ConsumePSIRecords(stoppingToken);
        await Task.WhenAll(gaConsumerTasK, psiConsumerTask);
    }

    private async Task ConsumeGARecords(CancellationToken stoppingToken)
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

    private async Task ConsumePSIRecords(CancellationToken stoppingToken)
    {
        _logger.LogInformation("START CONSUMING PSI Records............");
        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<
            IRawAnalyticRecordConsumers<PSIRecord>
        >();

        await handler.Consume(
            _rabbitMqSettings.Queues.PSI,
            _rabbitMqSettings.Queues.PSI,
            stoppingToken
        );
    }
}
