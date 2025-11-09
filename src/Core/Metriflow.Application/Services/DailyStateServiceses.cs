using IRepository.Generic;
using Metriflow.Application.interfaces;
using Metriflow.Domain.Entities;
using Metriflow.DTOs;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.Services;

public class DailyStateServices : IDailyStateServices
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBaseRepository<DailyStat> _dailyStateRepository;
    private readonly ILogger<DailyStateServices> _logger;

    public DailyStateServices(
        IBaseRepository<DailyStat> dailyStateRepository,
        ILogger<DailyStateServices> logger,
        IUnitOfWork unitOfWork
    )
    {
        // _dailyStateRepository = _dailyStateRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _dailyStateRepository = _unitOfWork.GetRepository<DailyStat>();
    }

    public async Task<DailyStat> CreateDailyStat(
        List<CombinedAnalyticsMessage> combinedAnalyticsMessages
    )
    {
        var tm = new TimeOnly(0, 0);
        var dailyState = new DailyStat
        {
            TotalUsers = combinedAnalyticsMessages.Sum(r => r.Users),
            TotalViews = combinedAnalyticsMessages.Sum(r => r.Views),
            TotalSessions = combinedAnalyticsMessages.Sum(r => r.Sessions),
            AvgPerformance = combinedAnalyticsMessages.Average(rc => rc.PerformanceScore),
            ReceivedAt = DateTime.UtcNow,
            Date = combinedAnalyticsMessages[0].Date.ToDateTime(tm),
        };
        await _dailyStateRepository.CreateAsync(dailyState);

        return dailyState;
    }
}
