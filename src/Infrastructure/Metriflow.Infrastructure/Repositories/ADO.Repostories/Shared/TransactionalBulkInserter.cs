using System.Data;
using IRepository;
using Metriflow.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Repositories.Ado;

internal static class TransactionalBulkInserter
{
    public static async Task InsertAsync(
        MetriflowDbContext context,
        ITrackTableCountRepository trackTableCountRepository,
        ILogger logger,
        string destinationTableName,
        string rowsCountTableName,
        IDataReader reader,
        Action<SqlBulkCopy> configureMappings,
        int rowCount,
        string operationName)
    {
        try
        {
            var dbTransaction = (await context.Database.BeginTransactionAsync()).GetDbTransaction();
            var connection =
                (SqlConnection)dbTransaction.Connection
                ?? throw new InvalidOperationException("No active transaction");

            using var bulkCopy = new SqlBulkCopy(
                connection,
                SqlBulkCopyOptions.TableLock,
                (SqlTransaction)dbTransaction);

            bulkCopy.DestinationTableName = destinationTableName;
            configureMappings(bulkCopy);

            await bulkCopy.WriteToServerAsync(reader);
            await trackTableCountRepository.AlterTableRowsCountAsync(rowsCountTableName, rowCount);
            await context.Database.CommitTransactionAsync();

            logger.LogInformation("Successfully bulk inserted {Count} {Operation}", rowCount, operationName);
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to bulk insert {Operation}", operationName);
            logger.LogError(ex, ex.Message);
            try
            {
                await context.Database.RollbackTransactionAsync();
            }
            catch (Exception rollbackEx)
            {
                logger.LogError(rollbackEx, "Failed to rollback {Operation} transaction", operationName);
            }

            throw;
        }
    }
}
