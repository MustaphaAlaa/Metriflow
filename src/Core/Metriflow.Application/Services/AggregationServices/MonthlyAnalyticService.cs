using Metriflow.Application.Interfaces;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.Services;


[ServiceRegistration(lifetime: ServiceLifetime.Scoped, serviceType: typeof(IMonthlyAnalyticService))]
public class MonthlyAnalyticService(ILogger<MonthlyAnalyticService> logger)
    : IMonthlyAnalyticService
{
    private readonly ILogger<MonthlyAnalyticService> logger = logger;

    public MonthlyAnalytic NormalizeMonthlyAnalytic(List<PageAnalytics> data)
    {
        if (data is null || data.Contains(null))
            throw new NullReferenceException("$$$$$Input data for NormalizeMonthlyAnalytic is null");

        MonthlyAnalytic monthlyAnalytic = new()
        {
            PageId = data[0].PageId,
            YearMonth = NormalizeRawAnalyticDate(data[0].Date),
            AvgPerformance = data.Average(rawData => rawData.PerformanceScore),
            TotalSessions = data.Sum(rawData => rawData.Sessions),
            TotalViews = data.Sum(rawData => rawData.Views),
            TotalUsers = data.Sum(rawData => rawData.Users),
        };
        return monthlyAnalytic;
    }

    DateTime NormalizeRawAnalyticDate(DateTime date) => new DateTime(date.Year, date.Month, 1,0,0,0, DateTimeKind.Utc);
}
