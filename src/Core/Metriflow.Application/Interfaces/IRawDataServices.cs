using Metriflow.Domain.Entities;
using Metriflow.DTOs;

namespace Metriflow.Application.interfaces;

public interface IRawDataServices
{
    Task<RawData> CreateRawData(CombinedAnalyticsMessage combinedAnalyticsMessage, Page page);
}