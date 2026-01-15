using System.ComponentModel;

namespace Metriflow.Domain.Entities.Enums;

public enum enAggregationLevel
{
    None = 0,
    [Description("Interval")]
    Interval,
    [Description("Daily")]
    Daily,
    [Description("Weekly")]
    Weekly,
    [Description("Monthly")]
    Monthly,
    [Description("Quarterly")]
    Quarterly,
    [Description("Yearly")]
    Yearly,
}