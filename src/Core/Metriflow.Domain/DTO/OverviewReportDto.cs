namespace Metriflow.DTOs;

public class OverviewReportDto
{
    public long TotalUsers { get; set; }
    public long TotalSessions { get; set; }
    public long TotalViews { get; set; }
    public double AvgPerformance { get; set; }
}
