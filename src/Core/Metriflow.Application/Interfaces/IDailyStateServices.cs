using Metriflow.Domain.Entities;
using Metriflow.DTOs;

namespace Metriflow.Application.interfaces;

public interface IDailyStateServices
{
    Task<DailyStat> CreateDailyStat(List<CombinedAnalyticsMessage> combinedAnalyticsMessages);
}