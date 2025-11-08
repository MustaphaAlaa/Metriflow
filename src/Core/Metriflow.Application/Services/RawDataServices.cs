using IRepository.Generic;
using Metriflow.Application.interfaces;
using Metriflow.Domain.Entities;
using Metriflow.DTOs;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.Services;

public class RawDataServices : IRawDataServices
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBaseRepository<RawData> _rawDataRepository;
    private readonly ILogger<RawDataServices> _logger;

    public RawDataServices(IBaseRepository<Page> pageRepository,
        IBaseRepository<RawData> rawDataRepo,
        ILogger<RawDataServices> logger
        ,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _rawDataRepository = _unitOfWork.GetRepository<RawData>();
    }

    public async Task<RawData> CreateRawData(CombinedAnalyticsMessage combinedAnalyticsMessage, Page page)
    {
        // IValidation.Validate()  ... Validate Page && Date Unique, etc...

        var tm = new TimeOnly(0,0);

        var rawData = await _rawDataRepository.CreateAsync(new RawData
        {
            PageId = page.Id,
            LCP_ms = combinedAnalyticsMessage.LCP_ms,
            PerformanceScore = combinedAnalyticsMessage.PerformanceScore,
            Users = combinedAnalyticsMessage.Users,
            Sessions = combinedAnalyticsMessage.Sessions,
            Views = combinedAnalyticsMessage.Views,
            Date = combinedAnalyticsMessage.Date.ToDateTime(tm),
        });
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation($"RawData Created Id => {rawData.Id} =>> {rawData.Date}  ");

        return rawData;
    }
}