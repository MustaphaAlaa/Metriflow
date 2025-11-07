using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Metriflow.Domain;

public class PageDailyStats
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public string Page { get; set; }
    public int TotalUsers { get; set; }
    public int TotalSessions { get; set; }
    public int TotalViews { get; set; }
    public double AvgPerformance { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}
