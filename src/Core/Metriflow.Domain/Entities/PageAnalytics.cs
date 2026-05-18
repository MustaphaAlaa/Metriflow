using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Metriflow.Domain.Entities.Enums;

namespace Metriflow.Domain.Entities;

public class PageAnalytics : AnalyticsData
{
    public DateTime Date { get; set; }
    public DateOnly DateOnly { get; set; }

    [ForeignKey("TimeInterval")] public int Interval { get; set; }
    public TimeInterval TimeInterval { get; set; }
}
