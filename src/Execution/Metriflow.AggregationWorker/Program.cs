using Infrastructure.Extensions;
using Metriflow.AggregationWorker.Interfaces;
using Metriflow.AggregationWorker.Interfaces.Correlation;
using Metriflow.AggregationWorker.Services;
using Metriflow.AggregationWorker.Workers; 
using Metriflow.Application.Entities;
using Metriflow.Application.Extensions;
using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Workers;
using Metriflow.Infrastructure;
using Metriflow.Messages.Connections;
using Metriflow.Messages.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<RawDataWorker>();

// builder.Services.AddHostedService<PagesAnalyticWorker>();
// builder.Services.AddHostedService<IntervalAnalyticsWorker>();
// builder.Services.AddHostedService<DailyAnalyticsWorker>();
// builder.Services.AddHostedService<MonthlyAnalyticsWorker>();
// builder.Services.AddHostedService<YearlyAnalyticsWorker>();

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));


builder.Services.AddSingleton<IRawDataConsumer, RawDataConsumer>();

// Register the generic message handler as scoped (open generic registration)
builder.Services.AddScoped(typeof(IConsumerMessageHandler<>), typeof(ConsumerMessageHandler<>));


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