namespace Metriflow.Domain.Entities;

public class DailyAnalytics : AggregateAnalytics
{

    public DateOnly Date { get; set; }
    public DateTime ReceivedAt { get; set; }

}
