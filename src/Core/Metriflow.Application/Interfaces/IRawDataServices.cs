using Metriflow.Domain.Entities;
using Metriflow.DTOs;

namespace Metriflow.Application.interfaces;

public interface IRawDataServices
{
    Task<RawData> NormalizeRawData(CombinedAnalyticsMessage combinedAnalyticsMessage, Page page);
}