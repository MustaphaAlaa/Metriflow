namespace Metriflow.Application.Entities;

public class QueueSettings
{
    public string IntervalAggregation { get; set; } = string.Empty;
    public string DailyAggregation { get; set; } = string.Empty;
    public string MonthlyAggregation { get; set; } = string.Empty;
    public string YearlyAggregation { get; set; } = string.Empty;
    public string AggregationCompleted { get; set; } = string.Empty;
    public string AggregationFailed { get; set; } = string.Empty;
    public string GA { get; set; } = string.Empty;
    public string PSA { get; set; } = string.Empty;
    public string Correlation { get; set; } = string.Empty;
}

public class StartAggregationMessage
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public AggregationType Type { get; set; }
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
}

public class AggregationCompletedMessage
{
    public Guid CorrelationId { get; set; }
    public AggregationType CompletedType { get; set; }
    public int ProcessedCount { get; set; }
    public DateTime CompletedAt { get; set; }
}

public class AggregationFailedMessage
{
    public Guid CorrelationId { get; set; }
    public AggregationType FailedType { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public DateTime FailedAt { get; set; }
}

public class ProcessedKey
{
    public DateTime Date { get; set; }
    public Guid PageId { get; set; }
}

public enum AggregationType
{
    Records, //  It's not an aggregation, but I'll leave it as it for now.
    Page,   //  It's not an aggregation, but I'll leave it as it for now.
    Interval,
    Daily,
    Monthly,
    Yearly
}