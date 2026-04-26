using IRepository.Generic;
using Metriflow.Application.Interfaces;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities.Workers;
using Metriflow.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

// Adapter for GA Records
[ServiceRegistration(lifetime: ServiceLifetime.Scoped, typeof(IRecordBatchSaver<GARecord>))]
public class GaRecordSaver : IRecordBatchSaver<GARecord>
{
    private readonly IRawDataRepository _repository;

    public GaRecordSaver(IRawDataRepository repository) => _repository = repository;

    public Task SaveBulkAsync(List<List<GARecord>> batch, int totalCount) =>
        _repository.AddGaRecordsBulk(batch, totalCount);
}
