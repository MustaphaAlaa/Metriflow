using System.ComponentModel.DataAnnotations;

namespace Metriflow.Domain.Entities;

public class RangeAnalytics : AggregateAnalytics
{
    // [Key]
    // public Guid Id { get; set; }
    public DateTime From { get; set; } 
    public DateTime To { get; set; }
}