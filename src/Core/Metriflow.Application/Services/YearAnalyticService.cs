using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.Services;


[ServiceRegistration(lifetime: ServiceLifetime.Scoped, serviceType: typeof(IYearAnalyticService))]
public class YearAnalyticService(ILogger<YearAnalyticService> logger) : IYearAnalyticService
{
    private readonly ILogger<YearAnalyticService> logger = logger;

    public YearlyAnalytics NormalizeYearlyAnalytic(
        List<MonthlyAnalytic> monthData
    )
    {
        if (monthData is null || monthData.Count < 12)
            return null;

        YearlyAnalytics yearlyAnalytics = new()
        {
            PageId = monthData[0].PageId,
            Year = monthData[0].YearMonth.Year,
            AvgPerformance = monthData.Average(data => data.AvgPerformance),
            TotalSessions = monthData.Sum(data => data.TotalSessions),
            TotalViews = monthData.Sum(data => data.TotalViews),
            TotalUsers = monthData.Sum(data => data.TotalUsers),
        };
        return yearlyAnalytics;
    }


}