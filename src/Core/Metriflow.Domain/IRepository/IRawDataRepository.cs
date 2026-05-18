using Metriflow.Domain.Entities.Workers;

namespace IRepository.Generic;

public interface IRawDataRepository
{
    Task ExecuteStagedProcedures();
    Task AddGaRecordsBulk(List<List<GARecord>> gaRecords, int count);
    Task AddPSARecordsBulk(List<List<PSARecord>> PSARecords, int count);
    Task ExecuteAnalyticsPagesCorrelationAsync();
}
