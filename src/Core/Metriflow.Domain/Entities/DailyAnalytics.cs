namespace Metriflow.Domain.Entities;

public class DailyAnalytics : AggregateAnalytics
{
    public Guid Id { get; set; }

    // Unique Key (Business) - Needs Fluent API mapping for composite key
    public DateTime Date { get; set; }
    public DateTime ReceivedAt { get; set; }

}
