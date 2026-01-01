using Metriflow.Domain.Entities;

namespace Metriflow.Application.Interfaces;

public interface IDailyStatCalculator
{
    Task<DailyStat> CalculateDailyStat(List<CombinedAnalyticsMessage> combinedAnalyticsMessages);
}
