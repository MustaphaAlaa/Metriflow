namespace Metriflow.Correlation.Worker.Interfaces;

/// <summary>
/// Defines utility or helper operations used by background services in the Correlation worker.
/// </summary>
public interface IHelper
{
    /// <summary>
    /// Execute the full matching/correlation flow (e.g., scan pending items, correlate records, and update state).
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes when the matching operation finishes.</returns>
    /// <remarks>
    /// This method may be invoked repeatedly by a background service and should be resilient to transient errors.
    /// It should honor internal cancellation (if supported by the implementation) and be safe to call concurrently only
    /// if the implementation supports concurrent execution.
    /// </remarks>
    Task MatchAll();
}
