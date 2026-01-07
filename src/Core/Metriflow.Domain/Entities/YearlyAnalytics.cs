using System.ComponentModel.DataAnnotations;
using Metriflow.Domain.interfaces;

namespace Metriflow.Domain.Entities;

public class YearlyAnalytics : AggregateAnalytics
{
    [Key]
    public Guid Id { get; set; }
    public int Year { get; set; } 
}