namespace Metriflow.Domain.Entities.Reports;

public class PageReport
{

    public string Path { get; set; }
    public long TotalUsers { get; set; }
    public long TotalSessions { get; set; }
    public long TotalViews { get; set; }
    public double AvgPerformance { get; set; }
}