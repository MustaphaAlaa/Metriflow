using Metriflow.Domain.Entities;

namespace Metriflow.Application.Interfaces;

public interface IPageServices
{
    Task<CombinedAnalyticsMessage> NormalizePage(CombinedAnalyticsMessage combinedAnalyticsMessage);
}
