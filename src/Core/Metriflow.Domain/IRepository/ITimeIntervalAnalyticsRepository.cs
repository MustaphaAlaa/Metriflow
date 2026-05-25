namespace IRepository.Generic;

public interface ITimeIntervalAnalyticsRepository
{
    Task<int> ExecuteAggregateTimeIntervalsAsync(CancellationToken stoppingToken);
}