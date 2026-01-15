using IRepository.Generic;
using Metriflow.Application.Interfaces;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Metriflow.Domain.Entities.Reports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.Services;

[ServiceRegistration(lifetime: ServiceLifetime.Scoped, serviceType: typeof(IPageServices))]
public class PageServices(
    IPageRepository pageRepository,
    ILogger<PageServices> logger,
    IUnitOfWork unitOfWork
) : IPageServices
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Page> GetAsync(string path)
    {
        var page = await pageRepository.RetrieveAsync(page => page.Path == path);
        return page;
    }

    public async Task<CombinedAnalyticsMessage> NormalizePage(
        CombinedAnalyticsMessage combinedAnalyticsMessage
    )
    {
        if (combinedAnalyticsMessage is null)
            return null;

        logger.LogInformation(
            $"Processing: {combinedAnalyticsMessage.Ticks} on Page {combinedAnalyticsMessage.Page}"
        );
        combinedAnalyticsMessage.Page = combinedAnalyticsMessage.Page;
        return combinedAnalyticsMessage;
    }

    public async Task<List<PageReport>> PageReport()
    {
        var report = await pageRepository.PageReportAsync();
        return report;
    }
}
