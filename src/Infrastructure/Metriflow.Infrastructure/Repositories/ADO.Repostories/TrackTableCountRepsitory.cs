using IRepository;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Repositories.Ado;

[ServiceRegistration( ServiceLifetime.Scoped, typeof(ITrackTableCountRepository))]
public class TrackTableCountRepository(MetriflowDbContext context, ILogger<TrackTableCountRepository> logger) : ITrackTableCountRepository
{
    public async Task<int> AlterTableRowsCountAsync(string tableName, int rowsCount)
    {
        var connection = (NpgsqlConnection)context.Database.CurrentTransaction.GetDbTransaction().Connection
                   ?? throw new InvalidOperationException("No active transaction");

        await using var cmd = new NpgsqlCommand("""
            UPDATE "TableRowsCounts" SET "RowsCount" =  "RowsCount" + @rowCount  WHERE  "TableName" = @tableName
            """, connection)
        {
            Parameters =
    {
        new("tableName", tableName)  ,
        new("rowCount", rowsCount)
    }
        };

        var affectedRows = await cmd.ExecuteNonQueryAsync();
        return affectedRows;

    }
}
