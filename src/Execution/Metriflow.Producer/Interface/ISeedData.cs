namespace Metriflow.Producers.Interfaces;

/// <summary>
/// Interface defining the contract for accessing seed data for analytics records.
/// </summary>
public interface ISeedData
{
    /// <summary>
    /// Gets the collection of Page Speed Insights (PSI) analytics records.
    /// </summary>
    public List<PSIRecord> PSIRecords { get; }

    /// <summary>
    /// Gets the collection of Google Analytics (GA) records.
    /// </summary>
    public List<GARecord> GARecords { get; }
}
