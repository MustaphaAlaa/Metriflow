namespace Metriflow.Correlation.Worker.Interfaces;

public interface IConsumerMessageHandler
{
    Task HandleIncomingRecordAsync<T>(string type, T record)
        where T : IAnalyticRecord;
}
