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
        var date = new DateTime(combinedAnalyticsMessage.Ticks);
        var rawData = new PageAnalytics
        {
            PageId = combinedAnalyticsMessage.Page,
            LcpMs = combinedAnalyticsMessage.LcpMs,
            PerformanceScore = combinedAnalyticsMessage.PerformanceScore,
            Users = combinedAnalyticsMessage.Users,
            Sessions = combinedAnalyticsMessage.Sessions,
            Views = combinedAnalyticsMessage.Views,
            Date = date,
            Intervals = TimeIntervalUtilities.GetTimeInterval(date.Hour),
        };

        return rawData;
    }

    public List<PageAnalytics> RecordsToPageAnalytics(IEnumerable<AggregateRecordsJoins> noneAggregateRecords)
    {
       logger.LogInformation("@@@@@@@RecordsToPageAnalytics method.");
      var records =   noneAggregateRecords.Select(record => new PageAnalytics()
        {  
                PageId = record.PageId,
                Page = null,
                LcpMs = record.PSIRecord.LCP_MS,
                PerformanceScore = record.PSIRecord.PerformanceScore,
                Users = record.GARecord.Users,
                Sessions = record.GARecord.Sessions,
                Views = record.GARecord.Views,
                Date = record.Date,
                Intervals = TimeIntervalUtilities.GetTimeInterval(record.Date.Hour),   
        }).ToList();
        logger.LogInformation("@@@@@@@Records to PageAnalytics is done.");
        return records;
    }
}
