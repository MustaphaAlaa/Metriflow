using Metriflow.Domain.Entities;

namespace Metriflow.Application.interfaces;

public interface IPageServices
{
    Task<CombinedAnalyticsMessage> NormalizePage(CombinedAnalyticsMessage combinedAnalyticsMessage);
}
