using IRepository.Generic;
using Metriflow.AggregationWorker.Interfaces;
using Metriflow.Application.interfaces;
using Metriflow.DTOs;

namespace Metriflow.AggregationWorker.Services;

public class _dailyStatCalculationOrchestrator : IDailyStatCalculationOrchestrator
{
    private readonly IDailyStatCalculator _dailyStatCalculator;
    private readonly IDailyStatRepository _dailyStatRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<_dailyStatCalculationOrchestrator> _logger;

    public _dailyStatCalculationOrchestrator(
        IDailyStatCalculator dailyStatCalculator,
        IDailyStatRepository dailyStatRepository,
        IUnitOfWork unitOfWork,
        ILogger<_dailyStatCalculationOrchestrator> logger
    )
    {
        _dailyStatCalculator = dailyStatCalculator;
        _dailyStatRepository = dailyStatRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task CalculateAndPersist(List<CombinedAnalyticsMessage> combinedAnalyticsMessages)
    {
        await ExecuteTransactionAsync(async () =>
        {
            var calculatedDailyState = await _dailyStatCalculator.CalculateDailyStat(
                combinedAnalyticsMessages
            );
            var dailyStat = await _dailyStatRepository.CreateAsync(calculatedDailyState);
        });
    }

    private async Task ExecuteTransactionAsync(Func<Task> action)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            await action();
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(e, "Raw data ingestion failed during transaction.");
            throw;
        }
    }
}
