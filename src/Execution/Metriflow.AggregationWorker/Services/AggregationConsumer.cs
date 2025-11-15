using IRepository.Generic;
using Metriflow.AggregationWorker.Interfaces;
using Metriflow.Application.interfaces;
using Metriflow.Domain.Entities;
using Metriflow.DTOs;

namespace Metriflow.AggregationWorker.Services;

public class AggregationConsumer : IAggregationConsumer
{
    private readonly IRawDataIngestionOrchestrator _rawDataIngestionOrchestrator;
    private readonly IDailyStatCalculationOrchestrator _dailyStatCalculationOrchestrator;

    public AggregationConsumer(
        IRawDataIngestionOrchestrator rawDataIngestionOrchestrator,
        IDailyStatCalculationOrchestrator dailyStatCalculationOrchestrator
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
