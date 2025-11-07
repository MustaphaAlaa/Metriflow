using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metriflow.Correlation.Worker.Interfaces;
using Metriflow.Domain;
using Metriflow.Messaging.interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Publishes <see cref="RawRecord"/> messages to the analytics RabbitMQ exchange.
/// </summary>
public class RawRecordProducer : IRowRecordProducer
{
    private readonly IRabbitMQProducer _rabbitMQProducer;
    private readonly string _exchangeName = "analytics.raw";
    private readonly ILogger<RawRecordProducer> _logger;

    /// <summary>
    /// Creates a new <see cref="RawRecordProducer"/> instance.
    /// </summary>
    /// <param name="rabbitMQProducer">The RabbitMQ producer to use for publishing.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public RawRecordProducer(IRabbitMQProducer rabbitMQProducer, ILogger<RawRecordProducer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _rabbitMQProducer =
            rabbitMQProducer ?? throw new ArgumentNullException(nameof(rabbitMQProducer));
    }

    /// <inheritdoc />
    public async Task PublishRawRecord(List<RawRecord> rawRecords)
    {
        if (rawRecords is null)
        {
            _logger.LogError("PublishRawRecord called with null rawRecords");
            throw new ArgumentNullException(nameof(rawRecords));
        }

        await _rabbitMQProducer.InitializeSharedChannelAsync(_exchangeName);
        await _rabbitMQProducer.PublishWithSharedChannelAsync(
            rawRecords,
            _exchangeName,
            "analytics.raw"
        );

        var logMessage = string.Join(",", rawRecords);
        _logger.LogInformation($"Published raw records to '{_exchangeName}': {logMessage}");
    }
}
