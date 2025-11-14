using IRepository.Generic;
using Metriflow.AggregationWorker.Interfaces;
using Metriflow.Application.interfaces;
using Metriflow.Domain.Entities;
using Metriflow.DTOs;

namespace Metriflow.AggregationWorker.Services;

public class AggregationConsumer : IAggregationConsumer
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPageServices _pageServices;
    private readonly IRawDataServices _rawDataServices;
    private readonly ILogger<AggregationConsumer> _logger;
    private readonly IPageRepository _pageRepository;
    private readonly IBaseRepository<RawData> _rawDataRepository;
    private readonly IDailyStatCalculator _dailyStatCalculator;
    private readonly IDailyStatRepository _dailyStatRepository;

    public AggregationConsumer(
        IRawDataServices rawDataServices,
        IDailyStatCalculator dailyStateServices,
        IDailyStatRepository dailyStatRepository,
        IPageServices pageServices,
        IPageRepository pageRepository,
        ILogger<AggregationConsumer> logger,
        IUnitOfWork unitOfWork
    )
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _dailyStatCalculator = dailyStateServices;
        _dailyStatRepository = dailyStatRepository;
        _rawDataServices = rawDataServices;
        _pageServices = pageServices;
        _pageRepository = pageRepository;
        _rawDataRepository = _unitOfWork.GetRepository<RawData>();
    }

    public async Task Consume(List<CombinedAnalyticsMessage> combinedAnalyticsMessages)
    {
        if (combinedAnalyticsMessages.Count == 0)
            return;

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            foreach (var msg in combinedAnalyticsMessages)
            {
                var normalizedPage = await _pageServices.NormalizePage(msg);
                var page = await _pageRepository.GetOrCreatePageAsync(normalizedPage);
                var normalizedRawData = await _rawDataServices.NormalizeRawData(msg, page);
                var rawData = await _rawDataRepository.CreateAsync(normalizedRawData);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation(
                    $"RawData Created Id => {normalizedRawData.Id} =>> {normalizedRawData.Date}"
                );
            }

            var calculatedDailyState = await _dailyStatCalculator.CalculateDailyStat(
                combinedAnalyticsMessages
            );
            var dailyStat = await _dailyStatRepository.CreateAsync(calculatedDailyState);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackAsync();
            Console.WriteLine(e);
            throw;
        }
    }
}
