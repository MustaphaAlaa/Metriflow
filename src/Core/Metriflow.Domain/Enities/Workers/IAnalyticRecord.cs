 
/// <summary>
/// A common interface for all analytic records to enable type-safe handling.
/// </summary>
public interface IAnalyticRecord
{
    DateTime Date { get; }
    string Page { get; }
}
