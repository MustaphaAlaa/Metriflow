using System.ComponentModel.DataAnnotations.Schema;

namespace Metriflow.Domain.Entities;

public abstract class AggregateAnalytics
{
    [ForeignKey("Page")] public int PageId { get; set; }
    public Page Page { get; set; }

    public long TotalUsers { get; set; }
    public long TotalSessions { get; set; }
    public long TotalViews { get; set; }
    public double AvgPerformance { get; set; }
}