namespace Metriflow.Domain.Entities;

public class MonthlyAnalytic : AggregateAnalytics
{
    public Guid Id { get; set; }
    public DateTime YearMonth { get; set; }
}