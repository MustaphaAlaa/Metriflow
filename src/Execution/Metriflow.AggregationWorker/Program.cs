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
using Metriflow.Messages.Connections;
using Metriflow.Messages.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt")
    .CreateBootstrapLogger();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog(
    (services, lc) =>
        lc
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
);

builder.Services.AddHostedService<RawDataWorker>();

builder.Services.AddHostedService<AggregationProgressWorker>();

// builder.Services.AddHostedService<PagesAnalyticWorker>();

// builder.Services.AddHostedService<IntervalAnalyticsWorker>();

// builder.Services.AddHostedService<DailyAnalyticsWorker>();
// builder.Services.AddHostedService<MonthlyAnalyticsWorker>();
// builder.Services.AddHostedService<YearlyAnalyticsWorker>();

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));

builder.Services.AddSingleton<IRawDataConsumer, RawDataConsumer>();
builder.Services.AddSingleton<IProducer, Producer>();

builder.Services.AddScoped(
    typeof(IRawDataConsumerMessageHandler<>),
    typeof(RawDataConsumerMessageHandler<>)
);
builder.Services.AddScoped<IPageAnalyticsOrchestration, PageAnalyticsOrchestration>();

builder.Services.AddInfrastructureLayer(builder.Configuration);
builder.Services.AddApplicationLayerDiServices();
builder.Services.AddRegisterReflection();
builder.Services.AddRabbitMqDi();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MetriflowDbContext>();
    dbContext.Database.Migrate();
}

host.Run();
