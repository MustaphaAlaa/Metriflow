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
    private readonly IDailyStateServices _dailyStateServices;
    private readonly IRawDataServices _rawDataServices;
    private readonly ILogger<AggregationConsumer> _logger;

    public AggregationConsumer(
        IRawDataServices rawDataServices,
        IDailyStateServices dailyStateServices,
        IPageServices pageServices,
        ILogger<AggregationConsumer> logger,
        IUnitOfWork unitOfWork
    )
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _dailyStateServices = dailyStateServices;
        _rawDataServices = rawDataServices;
        _pageServices = pageServices;
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
                var page = await this._pageServices.CreatePage(msg);

                await _rawDataServices.CreateRawData(msg, page);
            }

            var dailyState = _dailyStateServices.CreateDailyStat(combinedAnalyticsMessages);
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
