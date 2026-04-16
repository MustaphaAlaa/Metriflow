using System.ComponentModel.DataAnnotations;

namespace Metriflow.Domain.Entities;

public class TableRowsCount
{
    [Key]
    public int Id { get; set; }
    public string TableName { get; set; }
    public int RowsCount { get; set; }
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