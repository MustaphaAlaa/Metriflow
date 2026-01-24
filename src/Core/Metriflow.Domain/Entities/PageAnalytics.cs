using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Metriflow.Domain.Entities.Enums;

namespace Metriflow.Domain.Entities;

public class PageAnalytics : AnalyticsData
{
    [Key] public Guid Id { get; set; }
    [ForeignKey("PageId")] public int PageId { get; set; }
    public DateTime Date { get; set; }
    [ForeignKey("TimeInterval")] public int Intervals { get; set; }


    public Page Page { get; set; }
    public TimeInterval TimeInterval { get; set; }
}