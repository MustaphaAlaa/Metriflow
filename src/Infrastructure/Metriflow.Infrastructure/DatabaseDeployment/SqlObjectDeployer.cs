using Metriflow.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed class SqlObjectDeployer(
    MetriflowDbContext dbContext,
    ILogger<SqlObjectDeployer> logger)
{
    public async Task DeployAsync(CancellationToken ct = default)
    {
        var root = Path.Combine(
            AppContext.BaseDirectory,
            "DatabaseDeployment/SQL");

        if (!Directory.Exists(root))
        {
            logger.LogWarning("Database scripts folder not found: {Path}", root);
            return;
        }

        var files = Directory
            .GetFiles(root, "*.sql", SearchOption.AllDirectories)
            .OrderBy(x => x);


        foreach (var file in files)
        {
            logger.LogInformation("Deploying {File}", Path.GetFileName(file));

            var sql = await File.ReadAllTextAsync(file, ct);
            Console.WriteLine(sql);
            await dbContext.Database.ExecuteSqlRawAsync(sql, ct);
        }
    }
}