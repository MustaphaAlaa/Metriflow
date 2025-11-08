namespace Metriflow.DTOs;

public class CombinedAnalyticsMessage
{
    // The raw, combined data from GA and PSI
    public string Page { get; set; } // e.g., "/home"
    public DateOnly Date { get; set; } // e.g., 2025-10-20 (The date of the report)
    public long Users { get; set; }
    public long Sessions { get; set; }
    public long Views { get; set; }
    public double PerformanceScore { get; set; }
    public long LCP_ms { get; set; }

    public override string ToString()
    {
        return $" --- {this.Date} || {this.Page} --- ";
    }
}
