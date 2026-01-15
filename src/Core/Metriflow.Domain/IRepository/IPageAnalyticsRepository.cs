using Metriflow.Domain.Entities;

namespace IRepository.Generic;

public interface IPageAnalyticsRepository : IBaseRepository<PageAnalytics>
{
    
    IQueryable<PageAnalytics> GetUnaggregatedPageAnalytics(List<AggregationKey> aggregateKeys);
    
}