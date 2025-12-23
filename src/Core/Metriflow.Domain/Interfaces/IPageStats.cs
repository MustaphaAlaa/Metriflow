namespace Metriflow.Domain.interfaces;

public interface IPageStats
{
    int PageId { get; set; }
    int TotalUsers { get; set; }
    int TotalSessions { get; set; }
    int TotalViews { get; set; }
    double AvgPerformance { get; set; }
}
