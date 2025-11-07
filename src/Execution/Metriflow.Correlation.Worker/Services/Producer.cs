using Metriflow.Correlation.Worker.Interfaces;
using Metriflow.Domain;
using Metriflow.Messaging.interfaces;

public class RawRecordProducer : IRowRecordProducer
{
    private readonly IRabbitMQProducer _rabbitMQProducer;
    private readonly string _exchangeName = "analytics.raw";
    private ILogger<RawRecordProducer> _logger;

    public RawRecordProducer(IRabbitMQProducer rabbitMQProducer, ILogger<RawRecordProducer> logger)
    {
        _logger = logger;
        _rabbitMQProducer = rabbitMQProducer;
    }

    public async Task PublishRawRecord(RawRecord rawRecord)
    {
        await _rabbitMQProducer.InitializeSharedChannelAsync(_exchangeName);
        await _rabbitMQProducer.PublishWithSharedChannelAsync(
            rawRecord,
            _exchangeName,
            "analytics.raw"
        );
        _logger.LogInformation($"Raw Record → {rawRecord} is Published");
    }
}
