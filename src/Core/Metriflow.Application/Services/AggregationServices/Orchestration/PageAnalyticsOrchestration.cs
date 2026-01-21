using IRepository.Generic;
using Metriflow.Application.Interfaces;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Metriflow.Application.Services.Orchestration;

[ServiceRegistration(ServiceLifetime.Scoped, typeof(IPageAnalyticsOrchestration))]
public class PageAnalyticsOrchestration(
    IUnitOfWork unitOfWork,
    IAggregationProgressRepository aggregationProgressRepository,
    IPageAnalyticsServices pageAnalyticService
) : IPageAnalyticsOrchestration

{
    private readonly IBaseRepository<PageAnalytics> _pageAnalyticsRepository =
        unitOfWork.GetRepository<PageAnalytics>();


    public async Task<int>  CreatePageAnalyticsAsync()
    {
        var noneRecordsJoinsList = aggregationProgressRepository.GetNoneIntervalsAggregateRecords();

        if (!noneRecordsJoinsList.Any())
            return 0;

        var pagesAnalytics = pageAnalyticService.RecordsToPageAnalytics(noneRecordsJoinsList);

        await _pageAnalyticsRepository.CreateRangeAsync(pagesAnalytics);


        await unitOfWork.SaveChangesAsync();
        return pagesAnalytics.Count;
    }
}