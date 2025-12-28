namespace Metriflow.Domain.Entities;

public class CombinedAnalyticsMessage
{
    // The raw, combined data from GA and PSI
    public byte Page { get; set; }  
    public long Date { get; set; }  
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
