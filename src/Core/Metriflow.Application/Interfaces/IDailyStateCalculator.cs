using Metriflow.Domain.Entities;
using Metriflow.DTOs;

namespace Metriflow.Application.interfaces;

public interface IDailyStatCalculator
{
    Task<DailyStat> CalculateDailyStat(List<CombinedAnalyticsMessage> combinedAnalyticsMessages);
}
