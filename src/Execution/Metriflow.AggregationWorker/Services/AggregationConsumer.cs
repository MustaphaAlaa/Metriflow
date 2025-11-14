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
    private readonly IDailyStateCalculator _dailyStateCalculator;
    private readonly IRawDataServices _rawDataServices;
    private readonly ILogger<AggregationConsumer> _logger;
    IPageRepository _pageRepository;

    public AggregationConsumer(
        IRawDataServices rawDataServices,
        IDailyStateCalculator dailyStateServices,
        IPageServices pageServices,
        IPageRepository pageRepository,
        ILogger<AggregationConsumer> logger,
        IUnitOfWork unitOfWork
    )
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _dailyStateCalculator = dailyStateServices;
        _rawDataServices = rawDataServices;
        _pageServices = pageServices;
        _pageRepository = pageRepository;
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
                var normalizedPage = await this._pageServices.NormalizePage(msg);
                var page = await _pageRepository.GetOrCreatePageAsync(normalizedPage);
                await _rawDataServices.CreateRawData(msg, page);
            }

            var dailyState = _dailyStateCalculator.CreateDailyStat(combinedAnalyticsMessages);
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
