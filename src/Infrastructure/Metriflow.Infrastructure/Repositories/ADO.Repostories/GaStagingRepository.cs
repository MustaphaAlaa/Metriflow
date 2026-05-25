using System.Diagnostics;
using IRepository.Generic;
using Metriflow.Application.Services;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Repositories.Ado;

[ServiceRegistration(ServiceLifetime.Scoped, typeof(IGaStagingRepository))]
public class GaStagingRepository(MetriflowDbContext context, ILogger<GaStagingRepository> logger)
    : IGaStagingRepository
{
    public async Task ExecuteStageGaRecordsAsync(int processedCount, CancellationToken stoppingToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var batch = BatchingUtilities.ResolveRawDataBatchSize(processedCount);
        var sql = $"EXEC StageGARecords {batch};";

        var rowsProcessed = await SqlStoredProcedureExecutor.ExecuteScalarAsync(
            context,
            sql,
            commandTimeoutSeconds: 120,
            logger,
            successMessage: "GA raw data successfully loaded to staged tables",
            failureMessage: "Failed to load GA data to staged tables.", stoppingToken);
        stopwatch.Stop();

        var throughput =
            rowsProcessed / stopwatch.Elapsed.TotalSeconds;

        logger.LogInformation(
            "@TT@ GARecords → Processed {Rows} rows in {DurationMs} ms ({Throughput:N0} rows/sec)",
            rowsProcessed,
            stopwatch.ElapsedMilliseconds,
            throughput);
    }
}
