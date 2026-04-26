using Metriflow.Domain.Interfaces;

namespace Metriflow.Application.Interfaces;

public interface IRecordBatchSaver<T>
    where T : class, IAnalyticRecord
{
    Task SaveBulkAsync(List<List<T>> batch, int totalCount);
}
