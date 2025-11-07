using Metriflow.Correlation.Worker.Interfaces;
using Metriflow.Messaging.interfaces;
using StackExchange.Redis;

namespace Metriflow.Correlation.Worker;

/// <summary>
/// Coordinates consumption of GA and PSI messages by creating channels via an injected <see cref="IRabbitMQConsumer"/>
/// and attaching message handlers that persist messages (via IConsumerMessageHandler) into Redis.
/// </summary>
/// <remarks>
/// This class contains two consumption paths: GA and PSI. Each path:
/// - Creates a new channel via <see cref="IRabbitMQConsumer.CreateNewChannelAsync"/>,
/// - Starts a consume task with <see cref="IRabbitMQConsumer.ConsumeFromChannelAsync{T}"/> and provides a handler
///   that delegates to <see cref="IConsumerMessageHandler.MessageHandler{T}"/>.
/// </remarks>
public class Consumer : IConsumer
{
    private readonly ILogger<Consumer> _logger;
    private readonly IRabbitMQConsumer _consumer;

    private readonly IConsumerMessageHandler _consumerMessageHandler;

    public Consumer(
        ILogger<Consumer> logger,
        IRabbitMQConsumer consumer,
        IConsumerMessageHandler consumerMessageHandler
    )
    {
        _logger = logger;
        _consumer = consumer;
        _consumerMessageHandler = consumerMessageHandler;
    }

    /// <summary>
    /// Start consumption for all configured streams (GA and PSI).
    /// </summary>
    /// <param name="stoppingToken">Cancellation token used to stop consumption gracefully.</param>
    /// <returns>A Task representing the consumption setup. Note: the current implementation starts background consumption tasks but does not await them — see remarks.</returns>
    /// <remarks>
    /// Important: the existing implementation calls the internal `ConsumeGA` and `ConsumePSI` methods
    /// without awaiting or aggregating their returned tasks. As a result, the returned Task completes
    /// immediately and does not represent the lifetime of the background consumers. If the caller expects
    /// `Consume` to represent active consumption, consider awaiting the internal tasks or returning a Task
    /// that only completes when the consumption tasks complete or are canceled.
    /// </remarks>
    public async Task Consume(CancellationToken stoppingToken)
    {
        _logger.LogInformation("START CONSUMING............");
        this.ConsumeGA(stoppingToken);
        this.ConsumePSI(stoppingToken);
    }

    /// <summary>
    /// Start consuming GA messages on a dedicated channel.
    /// </summary>
    private async Task ConsumeGA(CancellationToken stoppingToken)
    {
        var analyticGA = await _consumer.CreateNewChannelAsync();

        var gaTask = _consumer.ConsumeFromChannelAsync(
            analyticGA,
            queueName: "GA-Queue",
            exchangeName: "analytics.raw",
            routingKey: "analytics.raw.ga",
            async (GARecord ga) =>
            {
                await Task.Delay(3000);
                await _consumerMessageHandler.HandleIncomingRecordAsync("ga", ga);
            },
            stoppingToken
        );
    }

    /// <summary>
    /// Start consuming PSI messages on a dedicated channel.
    /// </summary>
    private async Task ConsumePSI(CancellationToken stoppingToken)
    {
        var analyticPSI = await _consumer.CreateNewChannelAsync();

        var psiTask = _consumer.ConsumeFromChannelAsync(
            analyticPSI,
            queueName: "PSI-Queue",
            exchangeName: "analytics.raw",
            routingKey: "analytics.raw.psi",
            async (PSIRecord psi) =>
            {
                await Task.Delay(3000);
                await _consumerMessageHandler.HandleIncomingRecordAsync("psi", psi);
            },
            stoppingToken
        );
    }
}
