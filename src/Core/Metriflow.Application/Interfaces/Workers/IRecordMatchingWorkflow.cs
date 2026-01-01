using Metriflow.Domain.Entities;

namespace Metriflow.Application.Interfaces;

public interface IRecordMatchingWorkflow
{
    Task<IList<CombinedAnalyticsMessage>?> TryMatchAsync(List<string> keys);
}
