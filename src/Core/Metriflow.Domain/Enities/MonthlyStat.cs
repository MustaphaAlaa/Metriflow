using System.ComponentModel.DataAnnotations.Schema;
using Metriflow.Domain.interfaces;

namespace Metriflow.Domain.Entities;

public class MonthlyStat : IPageStats
{
    public Guid Id { get; set; }

    public string YearMonth { get; set; } // e.g., "202511". Should be unique.

    [ForeignKey("Page")]
    public int PageId { get; set; }
    public Page Page { get; set; }

    public int TotalUsers { get; set; }

    public int TotalSessions { get; set; }
    public int TotalViews { get; set; }
    public double AvgPerformance { get; set; }
}
