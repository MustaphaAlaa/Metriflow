using System.Data;
using IRepository;
using IRepository.Generic;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Metriflow.Domain.Entities.Workers;
using Metriflow.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Repositories.Ado;

[ServiceRegistration(ServiceLifetime.Scoped, typeof(IRawDataRepository))]
public class RawDataRepository(MetriflowDbContext context,
ITrackTableCountRepository trackTableCountRepository, ILogger<RawDataRepository> logger) : IRawDataRepository
{

    public async Task AddGaRecordsBulk(IEnumerable<GARecord> lst)
    {
        var connection = (NpgsqlConnection)context.Database.CurrentTransaction.GetDbTransaction().Connection
                       ?? throw new InvalidOperationException("No active transaction");

        var count = 0;
        using (var write = await connection.BeginBinaryImportAsync("""
        Copy "GARecords" ("Date", "PageId", "Users", "Views", "Sessions")
        FROM STDIN (FORMAT BINARY)
        """))
        {
            try
            {
                foreach (var item in lst)
                {
                    await write.StartRowAsync();



                    var date = new DateTime(item.Ticks, DateTimeKind.Utc);
                    write.Write(date, NpgsqlTypes.NpgsqlDbType.TimestampTz);

                    write.Write(item.PageId);
                    write.Write(item.Users, NpgsqlTypes.NpgsqlDbType.Bigint);
                    write.Write(item.Views, NpgsqlTypes.NpgsqlDbType.Bigint);
                    write.Write(item.Sessions, NpgsqlTypes.NpgsqlDbType.Bigint);
                    count++;

                }
                await write.CompleteAsync();

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "#####@@@@@@ Disposing GA lst");
                await write.DisposeAsync(); // force cancel COPY properly
                throw;
            }

        }

        await trackTableCountRepository.AlterTableRowsCountAsync("GARecords", count);


    }

    public async Task AddPsiRecordsBulk(IEnumerable<PSIRecord> lst)
    {
        var connection = (NpgsqlConnection)context.Database.CurrentTransaction.GetDbTransaction().Connection
                       ?? throw new InvalidOperationException("No active transaction");

        // if (connection.State != ConnectionState.Open)
        //     throw new InvalidOperationException("PostgreSQL connection required");
        var count = 0;
        using (var write = await connection.BeginBinaryImportAsync("""
        Copy "PSIRecords" ( "Date", "PageId", "PerformanceScore", "LCP_MS")
        FROM STDIN (FORMAT BINARY)
        """))
        {
            try
            {
                foreach (var item in lst)
                {
                    await write.StartRowAsync();
                    var date = new DateTime(item.Ticks, DateTimeKind.Utc);

                    write.Write(date, NpgsqlTypes.NpgsqlDbType.TimestampTz);
                    write.Write(item.PageId);
                    write.Write(item.PerformanceScore);
                    write.Write(item.LCP_MS, NpgsqlTypes.NpgsqlDbType.Bigint);
                    
                    count++;
                }
                await write.CompleteAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "#####@@@@@@ Disposing PSI lst");
                await write.DisposeAsync(); // force cancel COPY properly
                throw;
            }

        }

        await trackTableCountRepository.AlterTableRowsCountAsync("PSIRecords", count);
    }




}
