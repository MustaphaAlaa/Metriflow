using System.ComponentModel.DataAnnotations;
using Metriflow.Domain.interfaces;

namespace Metriflow.Domain.Entities;

public class YearlyAnalytics : AggregateAnalytics
{
    /// <summary>
    /// We care Only about year here;
    /// The key is year + PageId
    /// </summary>
    public int Year { get; set; }
}