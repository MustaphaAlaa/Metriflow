using Metriflow.Domain.Entities;

namespace Metriflow.Application.interfaces;

public interface IRawDataServices
{
    Task<RawData> NormalizeRawData(CombinedAnalyticsMessage combinedAnalyticsMessage, Page page);
}