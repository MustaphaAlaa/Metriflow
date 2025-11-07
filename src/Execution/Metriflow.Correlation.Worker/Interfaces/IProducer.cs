using Metriflow.Domain;

namespace Metriflow.Correlation.Worker.Interfaces;

public interface IRowRecordProducer
{
    Task PublishRawRecord(RawRecord rawRecord);
}
