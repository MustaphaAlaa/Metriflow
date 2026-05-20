using System.Data;
using IRepository;
using IRepository.Generic;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities.Workers;
using Metriflow.Domain.enums;
using Metriflow.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace Repositories.Ado;

[ServiceRegistration(ServiceLifetime.Scoped, typeof(IRawDataRepository))]
public class RawDataRepository(
    MetriflowDbContext context,
    ITrackTableCountRepository trackTableCountRepository,
    ILogger<RawDataRepository> logger
) : IRawDataRepository
{
    public async Task ExecuteStagedProceduresAsync()
    {
        try
        {
            // var dbTransaction = (await context.Database.BeginTransactionAsync()).GetDbTransaction();
            var connection =
                (SqlConnection)context.Database.GetDbConnection();
            // .;Connection
            // ?? throw new InvalidOperationException("No active transaction");
            var batch = (int)enBatchSizes.RawDataBaseBatch;
            var cmd = new SqlCommand(
                $"""
                 EXEC StagePSARecords {batch};
                 EXEC StageGARecords {batch};
                 """,
                connection
            );
            cmd.CommandTimeout = 120;

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }


            // cmd.Transaction = (SqlTransaction)dbTransaction;
            await cmd.ExecuteNonQueryAsync();
            // await context.Database.CommitTransactionAsync();
            logger.LogInformation("Raw data successfully loaded to staged tables");
        }
        catch (Exception ex)
        {
            logger.LogError("!!!!!!!!!!! Failed to load data to staged tables.  !!!!!!!!!!!!!!");
            logger.LogError(ex, ex.Message);
            // try
            // {
            //     await context.Database.RollbackTransactionAsync();
            // }
            // catch (Exception rollbackEx)
            // {
            //     logger.LogError(rollbackEx, "Failed to rollback data from staged tables");
            // }

            throw;
        }
    }

    public async Task AddGaRecordsBulkAsync(List<List<GARecord>> lst, int count)
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
                (SqlTransaction)dbTransaction
            );

            using var reader = new GARecordDataReader(lst);

            bulkCopy.DestinationTableName = "GARecords";

            bulkCopy.ColumnMappings.Add("Date", "Date");
            bulkCopy.ColumnMappings.Add("DateOnly", "DateOnly");
            bulkCopy.ColumnMappings.Add("PageId", "PageId");
            bulkCopy.ColumnMappings.Add("Users", "Users");
            bulkCopy.ColumnMappings.Add("Views", "Views");
            bulkCopy.ColumnMappings.Add("Sessions", "Sessions");
            bulkCopy.ColumnMappings.Add("IsCorrelation", "IsCorrelation");
            bulkCopy.ColumnMappings.Add("Hash", "Hash");

            await bulkCopy.WriteToServerAsync(reader);

            await trackTableCountRepository.AlterTableRowsCountAsync("GARecords", count);

            await context.Database.CommitTransactionAsync();
            logger.LogInformation("Successfully bulk inserted {Count} GA records", count);
        }
        catch (Exception ex)
        {
            logger.LogError("!!!!!!!!!!! Failed to bulk ga records !!!!!!!!!!!!!!");
            logger.LogError(ex, ex.Message);
            try
            {
                await context.Database.RollbackTransactionAsync();
            }
            catch (Exception rollbackEx)
            {
                logger.LogError(rollbackEx, "Failed to rollback GA transaction");
            }

            throw;
        }
    }

    public async Task AddPSARecordsBulkAsync(List<List<PSARecord>> lst, int count)
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
                (SqlTransaction)dbTransaction
            );

            using var reader = new PSARecordDataReader(lst);

            bulkCopy.DestinationTableName = "PSARecords";

            bulkCopy.ColumnMappings.Add("Date", "Date");
            bulkCopy.ColumnMappings.Add("DateOnly", "DateOnly");
            bulkCopy.ColumnMappings.Add("PageId", "PageId");
            bulkCopy.ColumnMappings.Add("PerformanceScore", "PerformanceScore");
            bulkCopy.ColumnMappings.Add("LCP_MS", "LCP_MS");
            bulkCopy.ColumnMappings.Add("IsCorrelation", "IsCorrelation");
            bulkCopy.ColumnMappings.Add("Hash", "Hash");

            await bulkCopy.WriteToServerAsync(reader);

            await trackTableCountRepository.AlterTableRowsCountAsync("PSARecords", count);
            await context.Database.CommitTransactionAsync();
            logger.LogInformation("Successfully bulk inserted {Count} PSA records", count);
        }
        catch (Exception ex)
        {
            logger.LogError("!!!!!!!!!!! Failed to bulk PSA records !!!!!!!!!!!!!!");
            logger.LogError(ex, ex.Message);
            try
            {
                await context.Database.RollbackTransactionAsync();
            }
            catch (Exception rollbackEx)
            {
                logger.LogError(rollbackEx, "Failed to rollback PSA transaction");
            }

            throw;
        }
    }


    public async Task ExecuteAnalyticsPagesCorrelationAsync()
    {
        try
        {
            var connection = (SqlConnection)context.Database.GetDbConnection();

            var cmd = new SqlCommand(
                $"""
                 EXEC ProcessCorrelatedPageAnalyticsBatch {(int)enBatchSizes.PageAnalyticsBatch}; 
                 """,
                connection
            );
            cmd.CommandTimeout = 120;


            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await cmd.ExecuteNonQueryAsync();
 
            logger.LogInformation("Staged raw data successfully correlated to PageAnalytics table");
        }
        catch (Exception ex)
        {
            logger.LogError(
                "!!!!!!!!!!! Failed to correlated raw data to PageAnalytics table.  !!!!!!!!!!!!!!"
            );
            logger.LogError(ex, ex.Message);
 
            throw;
        }
    }
}

// public class CorrelationRepository(
//     MetriflowDbContext context,
//     ILogger<RawDataRepository> logger
// )
// {
//     public async Task ExecuteAnalyticsPagesCorrelationAsync()
//     {
//         try
//         {
//             var dbTransaction = (await context.Database.BeginTransactionAsync()).GetDbTransaction();
//             var connection =
//                 (SqlConnection)dbTransaction.Connection
//                 ?? throw new InvalidOperationException("No active transaction");
//             var cmd = new SqlCommand(
//                 $"""
//                  EXEC ProcessCorrelatedPageAnalyticsBatch {(int)enBatchSizes.PageAnalyticsBatch}; 
//                  """,
//                 connection
//             );
//             cmd.Transaction = (SqlTransaction)dbTransaction;
//             cmd.CommandTimeout = 150;
//             await cmd.ExecuteNonQueryAsync();

//             await context.Database.CommitTransactionAsync();

//             logger.LogInformation("Staged raw data successfully correlated to PageAnalytics table");
//         }
//         catch (Exception ex)
//         {
//             logger.LogError(
//                 "!!!!!!!!!!! Failed to correlated raw data to PageAnalytics table.  !!!!!!!!!!!!!!"
//             );
//             logger.LogError(ex, ex.Message);
//             try
//             {
//                 await context.Database.RollbackTransactionAsync();
//             }
//             catch (Exception rollbackEx)
//             {
//                 logger.LogError(rollbackEx, "Failed to rollback data from staged tables");
//             }

//             throw;
//         }
//     }
// }