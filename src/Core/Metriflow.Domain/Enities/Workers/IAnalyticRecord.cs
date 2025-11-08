 
/// <summary>
/// A common interface for all analytic records to enable type-safe handling.
/// </summary>
public interface IAnalyticRecord
{
    DateOnly Date { get; }
    string Page { get; }
}
