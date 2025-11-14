using Metriflow.Domain.Entities;
using Metriflow.DTOs;

namespace Metriflow.Application.interfaces;

public interface IPageServices
{
    Task<CombinedAnalyticsMessage> NormalizePage(CombinedAnalyticsMessage combinedAnalyticsMessage);
}
