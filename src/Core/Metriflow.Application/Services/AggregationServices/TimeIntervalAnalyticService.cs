using Metriflow.Application.Interfaces;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.Services;

[ServiceRegistration(lifetime: ServiceLifetime.Scoped, serviceType: typeof(ITimeIntervalAnalyticService))]
public class TimeIntervalAnalyticService(ILogger<TimeIntervalAnalyticService> logger) : ITimeIntervalAnalyticService
{
    private readonly ILogger<TimeIntervalAnalyticService> logger = logger;

    public TimeIntervalAnalytic NormalizeTimeIntervalAnalytic(
        List<PageAnalytics> data
    )
    {
        if (data is null || data.Contains(null))
            return null;

        TimeIntervalAnalytic timeIntervalAnalytic = new()
        {
            PageId = data[0].PageId,
            TimeIntervalId = (byte)TimeIntervalUtilities.GetTimeInterval(data[0].Date.Hour),
            // Date = data[0].Date
        };
        
        AggregateUtilities.Aggregate(timeIntervalAnalytic, data);
        
        return timeIntervalAnalytic;
    }
}