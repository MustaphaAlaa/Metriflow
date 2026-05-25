using Infrastructure.Extensions;
using Metriflow.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructureLayer(builder.Configuration);

builder.Services.AddScoped<SqlObjectDeployer>();


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/")
    .CreateBootstrapLogger();


builder.Services.AddSerilog((services, lc) =>
    lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
);


var host = builder.Build();


using var scope = host.Services.CreateScope();

var services = scope.ServiceProvider;
try
{
    var dbContext = services.GetRequiredService<MetriflowDbContext>();
    dbContext.Database.Migrate();

    var deployer = services.GetRequiredService<SqlObjectDeployer>();
    await deployer.DeployAsync();
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    throw;
} 





