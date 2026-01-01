using Metriflow.Domain.Entities;

namespace Metriflow.Application.Interfaces;

public interface IRawDataServices
{
    Task<RawData> NormalizeRawData(CombinedAnalyticsMessage combinedAnalyticsMessage, Page page);
}