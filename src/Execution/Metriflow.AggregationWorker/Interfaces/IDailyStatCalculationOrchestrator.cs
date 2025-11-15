using Metriflow.DTOs;

namespace Metriflow.AggregationWorker.Interfaces;

public interface IDailyStatOrchestrator
{
    Task CalculateAndPersist(List<CombinedAnalyticsMessage> combinedAnalyticsMessages);
}
