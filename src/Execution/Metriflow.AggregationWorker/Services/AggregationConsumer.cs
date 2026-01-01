using IRepository.Generic;
using Metriflow.AggregationWorker.Interfaces;
using Metriflow.Application.Interfaces;
using Metriflow.Domain.Entities;
using Metriflow.DTOs;

namespace Metriflow.AggregationWorker.Services;

public class AggregationConsumer : IAggregationConsumer
{
    private readonly IRawDataIngestionOrchestrator _rawDataIngestionOrchestrator;
    private readonly IDailyStatOrchestrator _dailyStatCalculationOrchestrator;

    public AggregationConsumer(
        IRawDataIngestionOrchestrator rawDataIngestionOrchestrator,
        IDailyStatOrchestrator dailyStatCalculationOrchestrator
    )
    {
        _rawDataIngestionOrchestrator = rawDataIngestionOrchestrator;
        _dailyStatCalculationOrchestrator = dailyStatCalculationOrchestrator;
    }

    public async Task Consume(List<CombinedAnalyticsMessage> combinedAnalyticsMessages)
    {
        if (combinedAnalyticsMessages.Count == 0)
            return;

        await _rawDataIngestionOrchestrator.Ingest(combinedAnalyticsMessages);
        await _dailyStatCalculationOrchestrator.CalculateAndPersist(combinedAnalyticsMessages);
    }
}
