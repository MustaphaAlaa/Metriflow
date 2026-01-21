namespace Metriflow.Application.Interfaces;

public interface IPageAnalyticsOrchestration
{
    Task<int> CreatePageAnalyticsAsync();
}