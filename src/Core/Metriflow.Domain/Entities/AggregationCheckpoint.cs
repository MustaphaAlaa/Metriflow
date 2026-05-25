using System.ComponentModel.DataAnnotations;

namespace Metriflow.Domain.Entities;

public class AggregationCheckpoint
{
    [Key] public string PipelineName { get; set; }

    public DateTime LastProcessedAt { get; set; }
}