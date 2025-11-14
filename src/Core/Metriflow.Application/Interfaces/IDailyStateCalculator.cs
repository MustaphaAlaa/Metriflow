using Metriflow.Domain.Entities;
using Metriflow.DTOs;

namespace Metriflow.Application.interfaces;

public interface IDailyStateCalculator
{
    Task<DailyStat> CreateDailyStat(List<CombinedAnalyticsMessage> combinedAnalyticsMessages);
}
