using Metriflow.DTOs;

namespace Metriflow.AggregationWorker.Interfaces;

public interface IAggregationConsumer
{
    Task Consume(List<CombinedAnalyticsMessage> combinedAnalyticsMessages);
}
