using Azure;
using Infrastructure.Extensions;
using IRepository.Generic;
using Metriflow.AggregationWorker;
using Metriflow.AggregationWorker.Interfaces;
using Metriflow.AggregationWorker.Services;
using Metriflow.Application;
using Metriflow.Application.Entities;
using Metriflow.Application.Extensions;
using Metriflow.Application.interfaces;
using Metriflow.Application.Services;
using Metriflow.Infrastructure;
using Metriflow.Infrastructure.EntitiesConfigurations;
using Microsoft.EntityFrameworkCore;
using Repositories.Generic;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>();

builder.Services.AddSingleton<IRabbitMQConsumer, RabbitMQConsumer>();
builder.Services.AddSingleton<IRabbitMQProducer, RabbitMQProducer>();

builder.Services.AddApplicationLayer();
builder.Services.AddInfrastructureLayer();

builder.Services.AddScoped<IAggregationWorkerConsumer, AggregationWorkerConsumer>();
builder.Services.AddScoped<IAggregationConsumer, AggregationConsumer>();
builder.Services.AddScoped<IRawDataIngestionOrchestrator, RawDataIngestionOrchestrator>();
builder.Services.AddScoped<IDailyStatOrchestrator, DailyStatOrchestrator>();

builder.Services.AddDbContext<MetriflowDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("Sqlserver")),
    ServiceLifetime.Scoped
);
var host = builder.Build();
host.Run();
