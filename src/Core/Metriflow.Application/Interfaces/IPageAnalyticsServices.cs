using Metriflow.Domain.Entities;

namespace Metriflow.Application.Interfaces;

public interface IPageAnalyticsServices
{
    Task<PageAnalytics> NormalizeRawData(
        CombinedAnalyticsMessage combinedAnalyticsMessage,
        Page page
    );

    IEnumerable<PageAnalytics> RecordsToPageAnalytics(IEnumerable<AggregateRecordsJoins> noneAggregateRecords);
}