namespace Metriflow.Application.Interfaces;

public interface IDailyAnalyticsOrchestrator
{
    Task<int> AggregateDailyAnalyticsAsync();
}