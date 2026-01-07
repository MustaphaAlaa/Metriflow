using IRepository.Generic;
using Metriflow.AggregationWorker.Interfaces;
using Metriflow.Application.Interfaces;
using Metriflow.DTOs;

namespace Metriflow.AggregationWorker.Services;

public class DailyStatOrchestrator : IDailyStatOrchestrator
{
    private readonly IDailyAnalyticsService _dailyStatCalculator;
    private readonly IDailyAnalyticsRepository _dailyStatRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DailyStatOrchestrator> _logger;

    public DailyStatOrchestrator(
        IDailyAnalyticsService dailyStatCalculator,
        IDailyAnalyticsRepository dailyStatRepository,
        IUnitOfWork unitOfWork,
        ILogger<DailyStatOrchestrator> logger
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
