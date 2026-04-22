using IRepository;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Repositories.Ado;

[ServiceRegistration(ServiceLifetime.Scoped, typeof(ITrackTableCountRepository))]
public class TrackTableCountRepository(MetriflowDbContext context, ILogger<TrackTableCountRepository> logger) : ITrackTableCountRepository
{
    public async Task<int> AlterTableRowsCountAsync(string tableName, int rowsCount)
    {
        var dbTransaction = context.Database.CurrentTransaction.GetDbTransaction();
        var connection = (SqlConnection)dbTransaction.Connection
                   ?? throw new InvalidOperationException("No active transaction");

        await using var cmd = new SqlCommand("""
            UPDATE TableRowsCounts SET RowsCount =  RowsCount + @rowCount  WHERE  TableName = @tableName
            """, connection)
        {
            Parameters =
    {
        new("tableName", tableName)  ,
        new("rowCount", rowsCount)
    }
        };
        cmd.Transaction = (SqlTransaction)dbTransaction;
        var affectedRows = await cmd.ExecuteNonQueryAsync();
        return affectedRows;

    }
}
