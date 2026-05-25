using System.Data;
using Metriflow.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Repositories.Ado;

internal static class SqlStoredProcedureExecutor
{
    public static async Task ExecuteAsync(
        MetriflowDbContext context,
        string sql,
        int commandTimeoutSeconds,
        ILogger logger,
        string successMessage,
        string failureMessage,
        CancellationToken stoppingToken)
    {
        try
        {
            var connection = (SqlConnection)context.Database.GetDbConnection();
            await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = commandTimeoutSeconds };

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(stoppingToken);
            }

            await cmd.ExecuteNonQueryAsync(stoppingToken);
            logger.LogInformation(successMessage);
        }
        catch (Exception ex)
        {
            logger.LogError(failureMessage);
            logger.LogError(ex, ex.Message);
            throw;
        }
    }

    public static async Task<int> ExecuteScalarAsync(
        MetriflowDbContext context,
        string sql,
        int commandTimeoutSeconds,
        ILogger logger,
        string successMessage,
        string failureMessage,
        CancellationToken stoppingToken)
    {
        try
        {
            var connection = (SqlConnection)context.Database.GetDbConnection();
            await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = commandTimeoutSeconds };

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            var result = await cmd.ExecuteScalarAsync(stoppingToken);

            logger.LogInformation(successMessage);

            if (result != null && result != DBNull.Value)
            {
                return Convert.ToInt32(result);
            }

            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(failureMessage);
            logger.LogError(ex, ex.Message);
            throw;
        }
    }
}
