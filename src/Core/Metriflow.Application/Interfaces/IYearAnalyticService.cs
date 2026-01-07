using Metriflow.Domain.Entities;

namespace Metriflow.Application.Interfaces;

public interface IYearAnalyticService
{
    YearlyAnalytics NormalizeYearlyAnalytic(List<MonthlyAnalytic> monthData);
}
