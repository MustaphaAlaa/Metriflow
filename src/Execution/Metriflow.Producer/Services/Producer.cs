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


    public async Task ProducePSIAsync(IList<PSIRecord> data)
    {
        await this.Publish("PSI", data, "analytics.raw.psi");
    }


    public async Task ProduceGAAsync(IList<GARecord> data)
    {
        await this.Publish("GA", data, "analytics.raw.ga");
    }

    private async Task Publish<T>(string type, IList<T> data, string routingKey)
        where T : IAnalyticRecord
    {
        using var channel = await _rabbitMQProducer.CreateNewChannelAsync(_exchangeName);

        await Task.Delay(1500);
        var dayRecords = new List<T>();


        foreach (var record in data)
        {
            if (record.Date.Hour % 6 == 0 && dayRecords.Count > 0)
            {
                await _rabbitMQProducer.PublishToChannelAsync(
                    channel,
                    dayRecords,
                    _exchangeName,
                    routingKey
                );
                dayRecords.Clear();
            }

            _logger.LogInformation("{type} → {record}", type, record);
            dayRecords.Add(record);
        }

        if (dayRecords.Count > 0)
            await _rabbitMQProducer.PublishToChannelAsync(channel, dayRecords, _exchangeName, routingKey);

        dayRecords.Clear();
    }
}