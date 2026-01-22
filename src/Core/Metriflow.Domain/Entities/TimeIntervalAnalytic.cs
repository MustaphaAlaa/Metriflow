using System.ComponentModel.DataAnnotations.Schema;

namespace Metriflow.Domain.Entities;

public class TimeIntervalAnalytic : AggregateAnalytics
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    [ForeignKey("TimeInterval")]
     public int TimeIntervalId { get; set; }
     public TimeInterval TimeInterval { get; set; }
}