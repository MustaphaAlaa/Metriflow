using IRepository.Generic;
using Metriflow.Application.interfaces;
using Metriflow.Domain.Entities;
using Metriflow.DTOs;
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
        combinedAnalyticsMessage.Page = combinedAnalyticsMessage.Page.ToLower();

        string path = combinedAnalyticsMessage.Page;
        // Page? page = default;
        // lock (_lock)
        // {
        //     page = GetAsync(combinedAnalyticsMessage.Page).GetAwaiter().GetResult();
        //     if (page is null)
        //     {
        //         _logger.LogInformation(
        //             $"Creating Page: {path} --- Date: {combinedAnalyticsMessage.Date}"
        //         );
        //         page = _pageRepository
        //             .CreateAsync(new Page { Path = path })
        //             .GetAwaiter()
        //             .GetResult();
        //     }

        //     _unitOfWork.SaveChangesAsync().GetAwaiter().GetResult();
        // }

        // return page;

        return combinedAnalyticsMessage;
    }

    public async Task<List<PageReportDto>> PageReport()
    {
        var report = await _pageRepository.PageReportAsync();
        return report;
    }
}
