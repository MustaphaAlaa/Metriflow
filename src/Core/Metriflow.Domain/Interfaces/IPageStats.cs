namespace Metriflow.Domain.interfaces;

// 1. Interface for Code Structure
public interface IPageStats
{
    int PageId { get; set; }
    int TotalUsers { get; set; }
    int TotalSessions { get; set; }
    int TotalViews { get; set; }
    double AvgPerformance { get; set; }
}
