using IRepository.Generic;
using Metriflow.Application.Interfaces;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Metriflow.Application.Services.Orchestration;

[ServiceRegistration(ServiceLifetime.Scoped, typeof(IPageAnalyticsOrchestration))]
public class PageAnalyticsOrchestration(
    IUnitOfWork _unitOfWork,
    IAggregationProgressRepository _aggregationProgressRepository,
    IPageAnalyticsServices _pageAnalyticService
) : IPageAnalyticsOrchestration

{
    private readonly IBaseRepository<PageAnalytics> _pageAnalyticsRepository =
        _unitOfWork.GetRepository<PageAnalytics>();


    public async Task CreatePageAnalytics()
    {
        var noneRecordsJoinsList = _aggregationProgressRepository.GetNoneIntervalsAggregateRecords();

        if (!noneRecordsJoinsList.Any())
            return;

        var pagesAnalytics = _pageAnalyticService.RecordsToPageAnalytics(noneRecordsJoinsList);

        await _pageAnalyticsRepository.CreateRange(pagesAnalytics);


        await _unitOfWork.SaveChangesAsync();
    }
}