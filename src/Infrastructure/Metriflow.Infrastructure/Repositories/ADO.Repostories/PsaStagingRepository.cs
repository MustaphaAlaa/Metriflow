using System.Diagnostics;
using IRepository.Generic;
using Metriflow.Application.Services;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Repositories.Ado;

[ServiceRegistration(ServiceLifetime.Scoped, typeof(IPsaStagingRepository))]
public class PsaStagingRepository(MetriflowDbContext context, ILogger<PsaStagingRepository> logger)
    : IPsaStagingRepository
{
    public async Task ExecuteStagePsaRecordsAsync(int processedCount, CancellationToken stoppingToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var batch = BatchingUtilities.ResolveRawDataBatchSize(processedCount);
        var sql = $"EXEC StagePSARecords {batch};";

        var rowsProcessed = await SqlStoredProcedureExecutor.ExecuteScalarAsync(
            context,
            sql,
            commandTimeoutSeconds: 120,
            logger,
            successMessage: "PSA raw data successfully loaded to staged tables",
            failureMessage: "Failed to load PSA data to staged tables.", stoppingToken);

        var throughput =
            rowsProcessed / stopwatch.Elapsed.TotalSeconds;

        logger.LogInformation(
            "@TT@ PSARecords → Processed {Rows} rows in {DurationMs} ms ({Throughput:N0} rows/sec)",
            rowsProcessed,
            stopwatch.ElapsedMilliseconds,
            throughput);
    }
}
