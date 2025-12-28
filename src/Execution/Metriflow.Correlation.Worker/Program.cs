using Metriflow.Application.Entities;
using Metriflow.Application.interfaces;
using Metriflow.Correlation.Worker;
using Metriflow.Correlation.Worker.Interfaces;
using Metriflow.Messages.Connections;
using Metriflow.Messages.Extensions;
using Metriflow.Redis.Extensions;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<CorrelationWorker>();

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddScoped<IConsumerMessageHandler, ConsumerMessageHandler>();
builder.Services.AddScoped<ICorrelationConsumer, CorrelationConsumer>();

builder.Services.AddSingleton<IMessageBrokerConnection, RabbitMqConnection>();
builder.Services.AddRedisDI(builder.Configuration);
builder.Services.AddRabbitMqDi();
 
var host = builder.Build();

host.Run();
