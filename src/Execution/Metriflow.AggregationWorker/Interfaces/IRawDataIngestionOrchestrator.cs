using Metriflow.DTOs;

namespace Metriflow.AggregationWorker.Interfaces;

public interface IRawDataIngestionOrchestrator
{
    Task Ingest(List<CombinedAnalyticsMessage> combinedAnalyticsMessages);
}
