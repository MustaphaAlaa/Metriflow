 
public class PSIRecord : IAnalyticRecord
{
    public long Date { get; set; }
    public byte Page { get; set; }
    public int PerformanceScore { get; set; }
    public long LCP_MS { get; set; }

    public override string ToString()
    {
        return $"Date: {this.Date}, Page: {this.Page}, PerformanceScore: {this.PerformanceScore}, LCP: {this.LCP_MS}ms";
    }
}
