namespace Metriflow.Application.Interfaces;

public interface IYearlyAnalyticsOrchestrator
{
    Task<int> AggregateYearlyAnalyticsAsync();
}
