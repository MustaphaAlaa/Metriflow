using Metriflow.Domain.Entities.Enums;

namespace Metriflow.Domain.Entities;

public class TimeInterval
{
    public int Id { get; set; }

    public enTimeIntervals Interval { get; set; }

    public string IntervalDescription { get; set; } = null!;
}