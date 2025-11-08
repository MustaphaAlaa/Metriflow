using IRepository.Generic;
using Metriflow.Application.interfaces;
using Metriflow.Domain.Entities;
using Metriflow.DTOs;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.Services;

public class PageServices : IPageServices
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBaseRepository<Page> _pageRepository;
    private readonly ILogger<PageServices> _logger;

    public PageServices(IBaseRepository<Page> pageRepository,
        ILogger<PageServices> logger,
        IUnitOfWork unitOfWork)
    {
        // _pageRepository = pageRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _pageRepository = _unitOfWork.GetRepository<Page>();
    }

    public async Task<Page> GetAsync(string path)
    {
        var page = await _pageRepository.RetrieveAsync(page => page.Path == path);
        return page;
    }

    public async Task<Page> CreatePage(CombinedAnalyticsMessage combinedAnalyticsMessage)
    {
        if (combinedAnalyticsMessage is null)
            return null;
        
        _logger.LogInformation($"Porccessing: {combinedAnalyticsMessage.Date} on Page {combinedAnalyticsMessage.Page}");
        combinedAnalyticsMessage.Page = combinedAnalyticsMessage.Page.ToLower();
        string path = combinedAnalyticsMessage.Page;

        var page = await GetAsync(combinedAnalyticsMessage.Page);
        if (page is null)
        {
            _logger.LogInformation($"Creating Page: {path} --- Date: {combinedAnalyticsMessage.Date}");
            page = await _pageRepository.CreateAsync(new Page
            {
                Path = path
            });
        }

        // await _unitOfWork.SaveChangesAsync();
        return page;
    }
}