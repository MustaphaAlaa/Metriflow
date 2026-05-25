using Metriflow.Domain.Entities.Workers;

namespace IRepository.Generic;

public interface IPsaRecordRepository
{
    Task AddPSARecordsBulkAsync(List<List<PSARecord>> psaRecords, int count);
}
