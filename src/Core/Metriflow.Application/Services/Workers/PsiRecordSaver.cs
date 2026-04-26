using IRepository.Generic;
using Metriflow.Application.Interfaces;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities.Workers;
using Microsoft.Extensions.DependencyInjection;
// Adapter for PSI Records
[ServiceRegistration(lifetime: ServiceLifetime.Scoped, typeof(IRecordBatchSaver<PSIRecord>))]
public class PsiRecordSaver : IRecordBatchSaver<PSIRecord>
{
    private readonly IRawDataRepository _repository;

    public PsiRecordSaver(IRawDataRepository repository) => _repository = repository;

    public Task SaveBulkAsync(List<List<PSIRecord>> batch, int totalCount) =>
        _repository.AddPsiRecordsBulk(batch, totalCount);
}
