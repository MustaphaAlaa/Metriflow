using System.Collections;
using IRepository;
using IRepository.Generic;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Metriflow.Domain.Entities.Workers;
using Metriflow.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

namespace Repositories.Ado;

[ServiceRegistration(ServiceLifetime.Scoped, typeof(IRawDataRepository))]
public class RawDataRepository(
    MetriflowDbContext context,
    ITrackTableCountRepository trackTableCountRepository,
    ILogger<RawDataRepository> logger) : IRawDataRepository
{
    public async Task AddGaRecordsBulk(List<List<GARecord>> lst, int count)
    {
        try
        {
            var dbTransaction = (await context.Database.BeginTransactionAsync()).GetDbTransaction();
            var connection = (SqlConnection)dbTransaction.Connection
                             ?? throw new InvalidOperationException("No active transaction");

            using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, (SqlTransaction)dbTransaction);
            
            using var reader = new GARecordDataReader(lst);
             
            bulkCopy.DestinationTableName = "GARecords";
             
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

    public async Task AddPsiRecordsBulk(List<List<PSIRecord>> lst, int count)
    {
        try
        {
              
                
             
            var dbTransaction = (await context.Database.BeginTransactionAsync()).GetDbTransaction();
            var connection = (SqlConnection)dbTransaction.Connection
                             ?? throw new InvalidOperationException("No active transaction");

            using var bulkCopy = new SqlBulkCopy(
                connection,
                SqlBulkCopyOptions.TableLock,
                (SqlTransaction)dbTransaction);
            
            using var reader = new PSIRecordDataReader(lst);
            
            bulkCopy.DestinationTableName = "PSIRecords";
            
            await bulkCopy.WriteToServerAsync(reader);
            
            await trackTableCountRepository.AlterTableRowsCountAsync("PSIRecords", count);
            await context.Database.CommitTransactionAsync();
            logger.LogInformation("Successfully bulk inserted {Count} PSI records", count);
        }
        catch (Exception ex)
        {
            logger.LogError("!!!!!!!!!!! Failed to bulk psi records !!!!!!!!!!!!!!");
            logger.LogError(ex, ex.Message);
            try
            {
                await context.Database.RollbackTransactionAsync();
            }
            catch (Exception rollbackEx)
            {
                logger.LogError(rollbackEx, "Failed to rollback PSI transaction");
            }
            throw;
        }
    }
}