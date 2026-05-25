namespace IRepository.Generic;

public interface IPageAnalyticsCorrelationRepository
{
    Task<int> ExecuteAnalyticsPagesCorrelationAsync(CancellationToken stoppingToken);
}