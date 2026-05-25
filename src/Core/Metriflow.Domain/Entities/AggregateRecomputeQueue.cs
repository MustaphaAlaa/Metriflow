namespace Metriflow.Domain.Entities;

/// <summary>
/// AggregateRecomputeQueue should store the aggregated keys that needs to recomputed to handle the late-arrived data.
/// </summary>
public class AggregateRecomputeQueue
{
    public int PageId { get; set; }
    public DateOnly Date { get; set; }
    public int Interval { get; set; }
    public DateTime CreatedAt { get; set; }
}
