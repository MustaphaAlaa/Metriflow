namespace Metriflow.Domain.Entities;

public class StagingReadiness
{
    public required string Source { get; set; }
    public required Guid BatchId { get; set; }
    public bool Consumed { get; set; }

    public DateTime CreatedAt { get; set; }
}