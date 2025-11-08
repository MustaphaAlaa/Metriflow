using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metriflow.Correlation.Worker.Interfaces;
using Metriflow.Domain;
using Metriflow.DTOs;
using Metriflow.Application.interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Publishes <see cref="RawData"/> messages to the analytics RabbitMQ exchange.
/// </summary>
public class RawDataProducer : IRowDataProducer
{
    private readonly IRabbitMQProducer _rabbitMQProducer;
    private readonly string _exchangeName = "analytics.raw";
    private readonly ILogger<RawDataProducer> _logger;

    /// <summary>
    /// Creates a new <see cref="RawDataProducer"/> instance.
    /// </summary>
    /// <param name="rabbitMQProducer">The RabbitMQ producer to use for publishing.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public RawDataProducer(IRabbitMQProducer rabbitMQProducer, ILogger<RawDataProducer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _rabbitMQProducer =
            rabbitMQProducer ?? throw new ArgumentNullException(nameof(rabbitMQProducer));
    }

    /// <inheritdoc />
    public async Task PublishRawRecord(List<CombinedAnalyticsMessage> combineAnalyticsMessages)
    {
        if (combineAnalyticsMessages is null)
        {
            _logger.LogError("PublishRawRecord called with null rawRecords");
            throw new ArgumentNullException(nameof(combineAnalyticsMessages));
        }

        await _rabbitMQProducer.InitializeSharedChannelAsync(_exchangeName);
        await _rabbitMQProducer.PublishWithSharedChannelAsync(
            combineAnalyticsMessages,
            _exchangeName,
            "analytics.raw"
        );

        var logMessage = string.Join(",", combineAnalyticsMessages);
        _logger.LogInformation($"Published raw records to '{_exchangeName}': {logMessage}");
    }
}
