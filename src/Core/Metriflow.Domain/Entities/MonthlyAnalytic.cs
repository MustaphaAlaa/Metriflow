using System.ComponentModel.DataAnnotations;

namespace Metriflow.Domain.Entities;

public class MonthlyAnalytic : AggregateAnalytics
{
    [Key]
    public Guid Id { get; set; }
    public DateTime YearMonth { get; set; }
}