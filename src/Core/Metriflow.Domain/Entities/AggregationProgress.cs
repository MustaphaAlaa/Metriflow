using System.ComponentModel.DataAnnotations.Schema;

namespace Metriflow.Domain.Entities;

public class AggregationProgress
{
    [ForeignKey("PageId")]
    public int PageId { get; set; }
    public Page Page { get; set; }
    public DateTime Date { get; set; }
    public bool Interval { get; set; }
    public bool Correlation { get; set; }
    public bool Daily { get; set; }
    public bool Weekly { get; set; }
    public bool Monthly { get; set; }
    public bool Yearly { get; set; }
    public bool Quarterly { get; set; }
    public bool IsCompleted { get; set; }

}

// public class AggregationProgressV2
// {
//     public int PageId { get; set; }
//     public DateTime Date { get; set; }

//     public AggregationType Type { get; set; }

//     public bool IsCompleted { get; set; }
// }

// public enum AggregationType
// {
//     NoAggregation,
//     Daily,
//     Weekly,
//     Monthly,
//     Quarterly,
//     Yearly,
//     Correlation,
//     Interval
// }