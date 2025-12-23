using System.Diagnostics;
using System.Linq.Expressions;
using Metriflow.Application.interfaces;
using Metriflow.Domain.Entities.Workers;
using Metriflow.Domain.Interfaces;
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

    public Producer(IRabbitMQProducer rabbitMQProducer, ILogger<Producer> logger)
    {
        _logger = logger;
        _rabbitMQProducer = rabbitMQProducer;
    }

    public async Task ProducePSIAsync(PSIRecord[] data, IChannel channel)
    {
        await this.Publish("PSI", data, "analytics.raw.psi", channel);
    }

    public async Task ProduceGAAsync(GARecord[] data, IChannel channel)
    {
        await this.Publish("GA", data, "analytics.raw.ga", channel);
    }

    private async Task Publish<T>(string type, IList<T> data, string routingKey, IChannel channel)
        where T : IAnalyticRecord
    {
        await _rabbitMQProducer.PublishToChannelAsync(channel, data, _exchangeName, routingKey);
        _logger.LogInformation("Running on Thread: {thread}", Thread.CurrentThread.ManagedThreadId);
        _logger.LogInformation(
            "On routing key: {routingKey}, {type} → numbers of records has been published {record}, From: {from}, To: {to}",
            routingKey,
            type,
            data.Count,
            data[0].Date,
            data[data.Count - 1].Date
        );
    }
}
