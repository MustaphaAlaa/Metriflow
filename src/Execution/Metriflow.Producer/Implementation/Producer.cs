using Metriflow.Application.interfaces;
using Metriflow.Producers.Interfaces;
using Microsoft.Extensions.Logging;

namespace Metriflow.Producers.Implementation;

/// <summary>
/// Implements the message production functionality for analytics data.
/// Handles publishing of both GA and PSI records to RabbitMQ.
/// </summary>
public class Producer : IProducer
{
    private readonly IRabbitMQProducer _rabbitMQProducer;
    private readonly string _exchangeName = "analytics.raw";
    private ILogger<Producer> _logger;

    /// <summary>
    /// Initializes a new instance of the Producer class.
    /// </summary>
    /// <param name="rabbitMQProducer">The RabbitMQ producer instance for message publishing.</param>
    /// <param name="logger">The logger instance for logging producer events.</param>
    public Producer(IRabbitMQProducer rabbitMQProducer, ILogger<Producer> logger)
    {
        _logger = logger;
        _rabbitMQProducer = rabbitMQProducer;
    }

    /// <inheritdoc/>
    public async Task Produce(List<GARecord> gaData, List<PSIRecord> paData)
    {
        var gaTask = SentGA(gaData);
        var paTask = SentPSI(paData);

        await Task.WhenAll(gaTask, paTask);
    }

    /// <summary>
    /// Publishes PSI records to RabbitMQ with controlled delays.
    /// </summary>
    /// <param name="data">The PSI records to publish.</param>
    /// <remarks>
    /// Uses a dedicated channel and includes delays between messages (300ms)
    /// and an initial delay of 1000ms before starting.
    /// </remarks>
    private async Task SentPSI(List<PSIRecord> data)
    {
        using var PSIChannel = await _rabbitMQProducer.CreateNewChannelAsync(_exchangeName);
        await Task.Delay(1000);
        foreach (var psi in data)
        {
            await Task.Delay(2000);
            await _rabbitMQProducer.PublishToChannelAsync(
                PSIChannel,
                psi,
                _exchangeName,
                "analytics.raw.psi"
            );
            _logger.LogInformation($"PSI → {psi}");
        }
    }

    /// <summary>
    /// Publishes GA records to RabbitMQ with controlled delays.
    /// </summary>
    /// <param name="data">The GA records to publish.</param>
    /// <remarks>
    /// Uses a dedicated channel and includes delays between messages (200ms)
    /// and an initial delay of 1000ms before starting.
    /// </remarks>
    private async Task SentGA(List<GARecord> data)
    {
        using var GAChannel = await _rabbitMQProducer.CreateNewChannelAsync(_exchangeName);

        await Task.Delay(1000);
        foreach (var ga in data)
        {
            await Task.Delay(2000);
            await _rabbitMQProducer.PublishToChannelAsync(
                GAChannel,
                ga,
                _exchangeName,
                "analytics.raw.ga"
            );
            _logger.LogInformation($"GA → {ga}");
        }
    }
}
