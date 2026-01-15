using Metriflow.Domain.Entities;

namespace Metriflow.Application.Services;

public static class AggregateUtilities
{
    public static void Aggregate(AggregateAnalytics model, IEnumerable<AggregateAnalytics> data)
    {
        model.AvgPerformance = data.Average(data => data.AvgPerformance);
        model.TotalSessions = data.Sum(data => data.TotalSessions);
        model.TotalViews = data.Sum(data => data.TotalViews);
        model.TotalUsers = data.Sum(data => data.TotalUsers);
    }
    public static void Aggregate(AggregateAnalytics model, IEnumerable<AnalyticsData> data)
    {
        model.AvgPerformance = data.Average(data => data.PerformanceScore);
        model.TotalSessions = data.Sum(data => data.Sessions);
        model.TotalViews = data.Sum(data => data.Views);
        model.TotalUsers = data.Sum(data => data.Users);
    }
}