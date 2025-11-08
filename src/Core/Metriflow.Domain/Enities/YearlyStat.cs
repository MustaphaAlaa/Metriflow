using System.ComponentModel.DataAnnotations.Schema;
using Metriflow.Domain.interfaces;

namespace Metriflow.Domain.Entities;

public class YearlyStat  
{
    public Guid Id { get; set; }

    // Unique Key (Business)
    public int Year { get; set; }

    [ForeignKey("Page")]
    public int PageId { get; set; } // Implements IPageStats

    public int TotalUsers { get; set; }

    public Page Page { get; set; }
    public int TotalSessions { get; set; }
    public int TotalViews { get; set; }
    public double AvgPerformance { get; set; }
}
