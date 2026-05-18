using System.ComponentModel.DataAnnotations.Schema;

namespace Metriflow.Domain.Entities;

public class AnalyticsData
{
    [ForeignKey("Page")] public int PageId { get; set; }
    public Page Page { get; set; }
    public long Users { get; set; }
    public long Sessions { get; set; }
    public long Views { get; set; }
    public double PerformanceScore { get; set; }
    public long LcpMs { get; set; }
}