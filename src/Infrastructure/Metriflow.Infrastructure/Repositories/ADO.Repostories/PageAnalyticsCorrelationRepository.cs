using System.Diagnostics;
using IRepository.Generic;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.enums;
using Metriflow.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Repositories.Ado;

[ServiceRegistration(ServiceLifetime.Scoped, typeof(IPageAnalyticsCorrelationRepository))]
public class PageAnalyticsCorrelationRepository(
    MetriflowDbContext context,
    ILogger<PageAnalyticsCorrelationRepository> logger)
    : IPageAnalyticsCorrelationRepository
{
    public async Task<int> ExecuteAnalyticsPagesCorrelationAsync(CancellationToken stoppingToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var sql = $"""
                   EXEC ProcessCorrelatedPageAnalyticsBatch {(int)enBatchSizes.CorrelationBatch};
                   """;

        int rowsProcessed = await SqlStoredProcedureExecutor.ExecuteScalarAsync(
            context,
            sql,
            commandTimeoutSeconds: 120,
            logger,
            successMessage: "Staged raw data successfully correlated to PageAnalytics table",
            failureMessage: "Failed to correlate raw data to PageAnalytics table.",
            stoppingToken);

        stopwatch.Stop();

        var throughput =
            rowsProcessed / stopwatch.Elapsed.TotalSeconds;

        logger.LogInformation(
            "@TT@ PageAnalytics → Processed {Rows} rows in {DurationMs} ms ({Throughput:N0} rows/sec)",
            rowsProcessed,
            stopwatch.ElapsedMilliseconds,
            throughput);

        return rowsProcessed;
    }
}