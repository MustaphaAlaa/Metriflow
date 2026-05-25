using IRepository.Generic;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Repositories.Ado;

[ServiceRegistration(ServiceLifetime.Scoped, typeof(ITimeIntervalAnalyticsRepository))]
public class TimeIntervalAnalyticsRepository(
    MetriflowDbContext context,
    ILogger<TimeIntervalAnalyticsRepository
    > logger)
    : ITimeIntervalAnalyticsRepository
{
    public async Task<int> ExecuteAggregateTimeIntervalsAsync(CancellationToken stoppingToken)
    {
        var sql = $"""
                   EXEC AggregateTimeIntervals;
                   """;

        var rowsCount = await SqlStoredProcedureExecutor.ExecuteScalarAsync(
            context,
            sql,
            commandTimeoutSeconds: 120,
            logger,
            successMessage: "Data successfully aggregated from PageAnalytics table to  TimeIntervalsAnalytics table.",
            failureMessage: "Failed to aggregated from PageAnalytics table to  TimeIntervalsAnalytics table.",
            stoppingToken);
        return rowsCount;
    }
}