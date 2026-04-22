using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Metriflow.Infrastructure;

public class MetriflowDbContextFactory : IDesignTimeDbContextFactory<MetriflowDbContext>
{
    public MetriflowDbContext CreateDbContext(string[] args)
    {
        Console.WriteLine("FACTORY INVOKED");

        var basePath = Path.GetDirectoryName(typeof(MetriflowDbContext).Assembly.Location) ?? Directory.GetCurrentDirectory();
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<MetriflowDbContext>();
        var connectionString = configuration.GetConnectionString("sqlServer");
            // ?? "Host=localhost;Port=5432;Database=metriflow_db;Username=postgres;Password=postgres";
        
        optionsBuilder.UseSqlServer(connectionString);

        return new MetriflowDbContext(optionsBuilder.Options);
    }
}
