using IRepository.Generic;
using Metriflow.Application.Interfaces;
using Metriflow.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.Services;

public class RawDataServices : IRawDataServices
{
    private readonly ILogger<RawDataServices> _logger;

    public RawDataServices(ILogger<RawDataServices> logger)
    {
        _logger = logger;
    }

    public async Task<RawData> NormalizeRawData(
        CombinedAnalyticsMessage combinedAnalyticsMessage,
        Page page
    )
    {
        var tm = new TimeOnly(0, 0);
        var rawData = new RawData
        {
            PageId = page.Id,
            LCP_ms = combinedAnalyticsMessage.LCP_ms,
            PerformanceScore = combinedAnalyticsMessage.PerformanceScore,
            Users = combinedAnalyticsMessage.Users,
            Sessions = combinedAnalyticsMessage.Sessions,
            Views = combinedAnalyticsMessage.Views,
            //!!!! Date = combinedAnalyticsMessage.Date ,
        };

        return rawData;
    }
}
