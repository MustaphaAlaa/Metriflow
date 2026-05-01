using Metriflow.Domain.Entities.Workers;

namespace IRepository.Generic;

public interface IRawDataRepository
{
    Task ExecuteStagedProcedures();
    Task AddGaRecordsBulk(List<List<GARecord>> gaRecords, int count);
    Task AddPsiRecordsBulk(List<List<PSIRecord>> psiRecords, int count);
    Task ExecuteAnalyticsPagesCorrelationAsync();
}
