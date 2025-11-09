using Metriflow.Infrastructure;
using Metriflow.Application;
using Metriflow.Application.Entities;
using Metriflow.Application.interfaces;
using Metriflow.AggregationWorker;
using Metriflow.AggregationWorker.Services;
using Metriflow.AggregationWorker.Interfaces;
using Microsoft.EntityFrameworkCore;
using IRepository.Generic;
using Metriflow.Application.Extensions;
using Metriflow.Application.Services;
using Repositories.Generic;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>();

builder.Services.AddSingleton<IRabbitMQConsumer, RabbitMQConsumer>();
builder.Services.AddSingleton<IRabbitMQProducer, RabbitMQProducer>();

builder.Services.AddScoped<IAggregationWorkerConsumer, AggregationWorkerConsumer>();
builder.Services.AddScoped<IAggregationConsumer, AggregationConsumer>();

builder.Services.AddApplicationLayer();
// builder.Services.AddScoped<IPageServices, PageServices>();
// builder.Services.AddScoped<IRawDataServices, RawDataServices>();
// builder.Services.AddScoped<IDailyStateServices, DailyStateServices>();

builder.Services.AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>));
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();

builder.Services.AddDbContext<MetriflowDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("Sqlserver")),
    ServiceLifetime.Scoped
);
var host = builder.Build();
host.Run();