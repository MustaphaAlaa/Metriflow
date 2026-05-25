using IRepository;
using IRepository.Generic;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities.Workers;
using Metriflow.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Repositories.Ado;

[ServiceRegistration(ServiceLifetime.Scoped, typeof(IPsaRecordRepository))]
public class PsaRecordRepository(
    MetriflowDbContext context,
    ITrackTableCountRepository trackTableCountRepository,
    ILogger<PsaRecordRepository> logger)
    : IPsaRecordRepository
{
    public async Task AddPSARecordsBulkAsync(List<List<PSARecord>> psaRecords, int count)
    {
        using var reader = new PsaRecordDataReader(psaRecords);

        await TransactionalBulkInserter.InsertAsync(
            context,
            trackTableCountRepository,
            logger,
            destinationTableName: "PSARecords",
            rowsCountTableName: "PSARecords",
            reader,
            configureMappings: bulkCopy =>
            {
                bulkCopy.ColumnMappings.Add("Date", "Date");
                bulkCopy.ColumnMappings.Add("DateOnly", "DateOnly");
                bulkCopy.ColumnMappings.Add("PageId", "PageId");
                bulkCopy.ColumnMappings.Add("PerformanceScore", "PerformanceScore");
                bulkCopy.ColumnMappings.Add("LCP_MS", "LCP_MS");
                bulkCopy.ColumnMappings.Add("IsCorrelation", "IsCorrelation");
                bulkCopy.ColumnMappings.Add("Hash", "Hash");
            },
            rowCount: count,
            operationName: "PSA records");
    }
}
