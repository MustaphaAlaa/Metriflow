using Metriflow.Domain.Entities;

namespace Metriflow.Application.Interfaces;

public interface ITimeIntervalAnalyticService
{
    TimeIntervalAnalytic NormalizeTimeIntervalAnalytic(
        List<PageAnalytics> data
    );
}