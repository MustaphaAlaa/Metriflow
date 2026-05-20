using Metriflow.Domain.Entities.Workers;

namespace IRepository.Generic;

public interface IGaRecordRepository
{
    Task AddGaRecordsBulkAsync(List<List<GARecord>> gaRecords, int count);
}
