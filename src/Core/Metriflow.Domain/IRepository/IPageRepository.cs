using System.Linq.Expressions;
using Metriflow.Domain.Entities;
using Metriflow.Domain.Entities.Reports;

namespace IRepository.Generic;

public interface IPageRepository : IBaseRepository<Page>
{
    Task<List<PageReport>> PageReportAsync();
    Task<Page> GetOrCreatePageAsync(CombinedAnalyticsMessage combinedAnalyticsMessage);
}
