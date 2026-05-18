using IRepository.Generic;
using Metriflow.Application.Interfaces;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities.Workers;
using Microsoft.Extensions.DependencyInjection;
// Adapter for PSA Records
[ServiceRegistration(lifetime: ServiceLifetime.Scoped, typeof(IRecordBatchSaver<PSARecord>))]
public class PSARecordSaver : IRecordBatchSaver<PSARecord>
{
    private readonly IRawDataRepository _repository;

    public PSARecordSaver(IRawDataRepository repository) => _repository = repository;

    public Task SaveBulkAsync(List<List<PSARecord>> batch, int totalCount) =>
        _repository.AddPSARecordsBulk(batch, totalCount);
}
