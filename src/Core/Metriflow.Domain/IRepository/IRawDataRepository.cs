using Metriflow.Domain.Entities.Workers;

namespace IRepository.Generic;

public interface IRawDataRepository  
{
    Task AddGaRecordsBulk(IEnumerable<GARecord> gaRecords);
    Task AddPsiRecordsBulk(IEnumerable<PSIRecord> psiRecords);
}