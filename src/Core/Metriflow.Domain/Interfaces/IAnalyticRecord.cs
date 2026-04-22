namespace Metriflow.Domain.Interfaces;

/// <summary>
/// A common interface for all analytic records to enable type-safe handling.
/// </summary>
public interface IAnalyticRecord
{
    long Ticks { get; }
    int PageId { get; }
    Boolean IsCorrelation { get; }  
}
