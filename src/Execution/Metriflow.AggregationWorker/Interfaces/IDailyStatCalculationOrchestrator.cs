using Metriflow.DTOs;

namespace Metriflow.AggregationWorker.Interfaces;

public interface IDailyStatCalculationOrchestrator
{
    Task CalculateAndPersist(List<CombinedAnalyticsMessage> combinedAnalyticsMessages);
}
