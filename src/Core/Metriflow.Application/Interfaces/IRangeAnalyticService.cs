using Metriflow.Domain.Entities;

namespace Metriflow.Application.Interfaces;

public interface IRangeAnalyticService
{
    RangeAnalytics NormalizeRangeAnalytic(
        List<AggregateAnalytics> RangeData,
        DateTime From,
        DateTime To
    );
}
