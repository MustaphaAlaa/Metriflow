using System.ComponentModel.DataAnnotations.Schema;

namespace Metriflow.Domain.Entities;

public class AggregationProgress
{
    [ForeignKey("Page")]
    public int PageId { get; set; }
    public Page Page { get; set; }
    public DateTime Date { get; set; }
    public bool Interval { get; set; }
    public bool Daily { get; set; }
    public bool Weekly { get;set; }
    public bool Monthly { get; set; }
    public bool Yearly { get; set; }
    public bool Quarterly { get; set; }
    
}