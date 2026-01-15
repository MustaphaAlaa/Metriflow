namespace Metriflow.Domain.Entities;

public class CombinedAnalyticsMessage : AnalyticsData
{
    // The raw, combined data from GA and PSI
    public int Page { get; set; }  
    public long Ticks { get; set; }  


    public override string ToString()
    {
        return $" --- {this.Ticks} || {this.Page} --- ";
    }
}
