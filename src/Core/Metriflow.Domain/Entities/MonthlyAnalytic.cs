using System.ComponentModel.DataAnnotations;

namespace Metriflow.Domain.Entities;

public class MonthlyAnalytic : AggregateAnalytics
{
    /// <summary>
    /// Represent the year and month; I don't care about day here
    /// </summary>
    public DateOnly YearMonth { get; set; }
}