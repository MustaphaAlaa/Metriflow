using System.Threading;
using System.Threading.Tasks;
using Metriflow.Application.interfaces;
using Metriflow.Correlation.Worker.Interfaces;
using Microsoft.Extensions.Logging;

namespace Metriflow.Correlation.Worker;

/// <summary>
/// Top-level consumer that wires RabbitMQ consumer channels to message handling logic.
/// </summary>
public class CorrelationConsumer : ICorrelationConsumer
{
    private readonly ILogger<CorrelationConsumer> _logger;
    private readonly IRabbitMQConsumer _consumer;

    private readonly IConsumerMessageHandler _consumerMessageHandler;

    /// <summary>
    /// Creates a new <see cref="CorrelationConsumer"/>.
    /// </summary>
    public CorrelationConsumer(
        ILogger<CorrelationConsumer> logger,
        IRabbitMQConsumer consumer,
        IConsumerMessageHandler consumerMessageHandler
    )
    {
        _logger = logger;
        _consumer = consumer;
        _consumerMessageHandler = consumerMessageHandler;
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
        // var analyticGA = await _consumer.CreateNewChannelAsync();

        // var gaTask = _consumer.ConsumeFromChannelAsync(
        //     analyticGA,
        //     queueName: "GA-Queue",
        //     exchangeName: "analytics.raw",
        //     routingKey: "analytics.raw.ga",
        //     async (GARecord ga) =>
        //     {
        //         await Task.Delay(1000);
        //         await _consumerMessageHandler.HandleIncomingRecordAsync("ga", ga);
        //     },
        //     stoppingToken
        // );
        await this.ConsumeGeneric(
            queueName: "GA-Queue",
            routingKey: "analytics.raw.ga",
            async (List<GARecord> ga) =>
            {
                // await Task.Delay(1000);
                await _consumerMessageHandler.HandleIncomingRecordAsync("ga", ga);
            },
            stoppingToken
        );
    }

    /// <summary>
    /// Start consuming PSI messages on a dedicated channel and forward them to the handler.
    /// </summary>
    private async Task ConsumePSI(CancellationToken stoppingToken)
    {
        // var analyticPSI = await _consumer.CreateNewChannelAsync();

        // var psiTask = _consumer.ConsumeFromChannelAsync(
        //     analyticPSI,
        //     queueName: "PSI-Queue",
        //     exchangeName: "analytics.raw",
        //     routingKey: "analytics.raw.psi",
        //     async (PSIRecord psi) =>
        //     {
        //         await Task.Delay(3000);
        //         await _consumerMessageHandler.HandleIncomingRecordAsync("psi", psi);
        //     },
        //     stoppingToken
        // );

        await ConsumeGeneric(
            queueName: "PSI-Queue",
            routingKey: "analytics.raw.psi",
            async (List<PSIRecord> psi) =>
            {
                // await Task.Delay(3000);
                await _consumerMessageHandler.HandleIncomingRecordAsync("psi", psi);
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
        var analyticPSI = await _consumer.CreateNewChannelAsync();

        var psiTask = _consumer.ConsumeFromChannelAsync(
            analyticPSI,
            queueName,
            exchangeName: "analytics.raw",
            routingKey,
            dlg,
            stoppingToken
        );
    }
}
