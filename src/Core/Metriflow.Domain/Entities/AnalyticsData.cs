namespace Metriflow.Domain.Entities;

public class AnalyticsData
{
    public long Users { get; set; }
    public long Sessions { get; set; }          
    public long Views { get; set; }
    public double PerformanceScore { get; set; }
    public long LcpMs { get; set; }
}