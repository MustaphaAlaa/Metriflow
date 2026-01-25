namespace Metriflow.Application.Interfaces;

public interface IMonthlyAnalyticsOrchestrator
{
    Task<int> AggregateMonthlyAnalyticsAsync();
}
