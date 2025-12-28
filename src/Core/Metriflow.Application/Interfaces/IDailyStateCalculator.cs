using Metriflow.Domain.Entities;

namespace Metriflow.Application.interfaces;

public interface IDailyStatCalculator
{
    Task<DailyStat> CalculateDailyStat(List<CombinedAnalyticsMessage> combinedAnalyticsMessages);
}
