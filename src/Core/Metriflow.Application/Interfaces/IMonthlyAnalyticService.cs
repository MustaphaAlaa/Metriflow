using Metriflow.Domain.Entities;

namespace Metriflow.Application.Interfaces;

public interface IMonthlyAnalyticService
{
    MonthlyAnalytic NormalizeMonthlyAnalytic(List<PageAnalytics> data);
}
