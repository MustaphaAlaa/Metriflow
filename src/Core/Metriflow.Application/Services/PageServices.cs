using IRepository.Generic;
using Metriflow.Application.interfaces;
using Metriflow.Domain.Entities;
using Metriflow.Domain.Entities.Reports;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.Services;

public class PageServices : IPageServices
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPageRepository _pageRepository;
    private readonly ILogger<PageServices> _logger;
    private readonly Object _lock = new();

    public PageServices(
        IPageRepository pageRepository,
        ILogger<PageServices> logger,
        IUnitOfWork unitOfWork
    )
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _pageRepository = pageRepository;
    }

    public async Task<Page> GetAsync(string path)
    {
        var page = await _pageRepository.RetrieveAsync(page => page.Path == path);
        return page;
    }

    public async Task<CombinedAnalyticsMessage> NormalizePage(
        CombinedAnalyticsMessage combinedAnalyticsMessage
    )
    {
        if (combinedAnalyticsMessage is null)
            return null;

        _logger.LogInformation(
            $"Processing: {combinedAnalyticsMessage.Date} on Page {combinedAnalyticsMessage.Page}"
        );
        combinedAnalyticsMessage.Page = combinedAnalyticsMessage.Page;
        return combinedAnalyticsMessage;
    }

    public async Task<List<PageReport>> PageReport()
    {
        var report = await _pageRepository.PageReportAsync();
        return report;
    }
}
