using System.Diagnostics;
using System.Linq.Expressions;
using Metriflow.Application.interfaces;
using Metriflow.Producers.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

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
    public async Task Produce(List<GARecord> gaData, List<PSIRecord> psiData)
    {
        var gaTask = SendGaAsync(gaData);
        var paTask = SendPsiAsync(psiData);

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
    private async Task SendPsiAsync(List<PSIRecord> data)
    {
        await this.Publish("PSI", data, "analytics.raw.psi");
    }

    /// <summary>
    /// Publishes GA records to RabbitMQ with controlled delays.
    /// </summary>
    /// <param name="data">The GA records to publish.</param>
    /// <remarks>
    /// Uses a dedicated channel and includes delays between messages (200ms)
    /// and an initial delay of 1000ms before starting.
    /// </remarks>
    private async Task SendGaAsync(List<GARecord> data)
    {
        await this.Publish("GA", data, "analytics.raw.ga");
    }

    private async Task Publish<T>(string type, IList<T> list, string routingKey)
        where T : IAnalyticRecord
    {
        using var channel = await _rabbitMQProducer.CreateNewChannelAsync(_exchangeName);

        await Task.Delay(1500);
        var dayRecords = new List<T>();

        var buffer = new DateOnly();
    
        foreach (var record in list)
        {

            if (buffer < record.Date)
            {
                await _rabbitMQProducer.PublishToChannelAsync(
                    channel,
                    dayRecords,
                    _exchangeName,
                    routingKey
                );
                dayRecords.Clear();
                buffer = record.Date;
            }

            _logger.LogInformation("{type} → {record}", type, record);
            dayRecords.Add(record);
        }

        if (dayRecords.Count > 0)
            await _rabbitMQProducer.PublishToChannelAsync(channel, dayRecords, _exchangeName, routingKey);

        dayRecords.Clear();
    }
}
