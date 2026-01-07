using Metriflow.Application.Interfaces;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Metriflow.Domain.Entities.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.Services;

[ServiceRegistration(lifetime: ServiceLifetime.Scoped, serviceType: typeof(IPageAnalyticsServices))]
public class PageAnalyticsServices(ILogger<PageAnalyticsServices> logger) : IPageAnalyticsServices
{
    private readonly ILogger<PageAnalyticsServices> logger = logger;

    public async Task<PageAnalytics> NormalizeRawData(
        CombinedAnalyticsMessage combinedAnalyticsMessage,
        Page page
    )
    {
        var rawData = new PageAnalytics
        {
            PageId = page.Id,
            LCP_ms = combinedAnalyticsMessage.LCP_ms,
            PerformanceScore = combinedAnalyticsMessage.PerformanceScore,
            Users = combinedAnalyticsMessage.Users,
            Sessions = combinedAnalyticsMessage.Sessions,
            Views = combinedAnalyticsMessage.Views,
            Date = new DateTime(combinedAnalyticsMessage.Date),
            Intervals = GetTimeInterval(combinedAnalyticsMessage),
        };

        return rawData;
    }

    private enTimeIntervals GetTimeInterval(CombinedAnalyticsMessage combinedAnalyticsMessage)
    {
        var hour = new DateTime(combinedAnalyticsMessage.Date).Hour;
        var interval = hour switch
        {
            < 4 => enTimeIntervals.First,
            < 8 => enTimeIntervals.Second,
            < 12 => enTimeIntervals.Third,
            < 16 => enTimeIntervals.Fourth,
            < 20 => enTimeIntervals.Fifth,
            _ => enTimeIntervals.Sixth,
        };
        return interval;
    }
}
