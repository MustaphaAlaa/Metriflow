using Metriflow.Application.Interfaces;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.Services;

[ServiceRegistration(lifetime: ServiceLifetime.Scoped, serviceType: typeof(IRangeAnalyticService))]
public class RangeAnalyticService(ILogger<RangeAnalyticService> logger) : IRangeAnalyticService
{
    private readonly ILogger<RangeAnalyticService> logger = logger;

    public RangeAnalytics NormalizeRangeAnalytic(
        List<AggregateAnalytics> RangeData,
        DateTime From,
        DateTime To
    )
    {
        if (RangeData is null || RangeData.Count < 12)
            return null;

        RangeAnalytics rangeAnalytics = new()
        {
            PageId = RangeData[0].PageId,
            From = From,
            To = To,
            AvgPerformance = RangeData.Average(data => data.AvgPerformance),
            TotalSessions = RangeData.Sum(data => data.TotalSessions),
            TotalViews = RangeData.Sum(data => data.TotalViews),
            TotalUsers = RangeData.Sum(data => data.TotalUsers),
        };
        return rangeAnalytics;
    }
}
