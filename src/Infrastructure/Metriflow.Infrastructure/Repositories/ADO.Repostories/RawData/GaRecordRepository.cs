using IRepository;
using IRepository.Generic;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities.Workers;
using Metriflow.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Repositories.Ado;

[ServiceRegistration(ServiceLifetime.Scoped, typeof(IGaRecordRepository))]
public class GaRecordRepository(
    MetriflowDbContext context,
    ITrackTableCountRepository trackTableCountRepository,
    ILogger<GaRecordRepository> logger)
    : IGaRecordRepository
{
    public async Task AddGaRecordsBulkAsync(List<List<GARecord>> gaRecords, int count)
    {
        using var reader = new GARecordDataReader(gaRecords);

        await TransactionalBulkInserter.InsertAsync(
            context,
            trackTableCountRepository,
            logger,
            destinationTableName: "GARecords",
            rowsCountTableName: "GARecords",
            reader,
            configureMappings: bulkCopy =>
            {
                bulkCopy.ColumnMappings.Add("Date", "Date");
                bulkCopy.ColumnMappings.Add("DateOnly", "DateOnly");
                bulkCopy.ColumnMappings.Add("PageId", "PageId");
                bulkCopy.ColumnMappings.Add("Users", "Users");
                bulkCopy.ColumnMappings.Add("Views", "Views");
                bulkCopy.ColumnMappings.Add("Sessions", "Sessions");
                bulkCopy.ColumnMappings.Add("IsCorrelation", "IsCorrelation");
                bulkCopy.ColumnMappings.Add("Hash", "Hash");
            },
            rowCount: count,
            operationName: "GA records");
    }
}
