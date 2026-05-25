using Infrastructure.Extensions;
using Metriflow.AggregationWorker.Interfaces;
using Metriflow.AggregationWorker.Interfaces.Correlation;
using Metriflow.AggregationWorker.Services;
using Metriflow.AggregationWorker.Workers;
using Metriflow.Application.Entities;
using Metriflow.Application.Extensions;
using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Workers;
using Metriflow.Application.Services.Orchestration;
using Metriflow.Application.Services.Workers;
using Metriflow.Infrastructure;
using Metriflow.Messages.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructureLayer(builder.Configuration);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt")
    .CreateBootstrapLogger();


builder.Services.AddSerilog((services, lc) =>
    lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
);

builder.Services.AddHostedService<RawDataWorker>();

builder.Services.AddHostedService<StagedGaDataWorker>();
builder.Services.AddHostedService<StagedPsaDataWorker>();

builder.Services.AddHostedService<PagesAnalyticWorker>();

builder.Services.AddHostedService<TimeIntervalAnalyticsWorker>();


builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));

builder.Services.AddSingleton<IRawDataConsumer, RawDataConsumer>();
builder.Services.AddSingleton<IProducer, Producer>();

builder.Services.AddScoped(
    typeof(IRawDataConsumerMessageHandler<>),
    typeof(RawDataConsumerMessageHandler<>)
);
// builder.Services.AddScoped<IPageAnalyticsOrchestration, PageAnalyticsOrchestration>();

builder.Services.AddApplicationLayerDiServices();
builder.Services.AddRabbitMqDi();


builder.Services.AddRegisterReflection();


var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MetriflowDbContext>();
    dbContext.Database.Migrate();
}



host.Run();
