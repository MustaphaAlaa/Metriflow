using Metriflow.Correlation.Worker;
using Metriflow.Messaging;
using Metriflow.Messaging.Entities;
using Metriflow.Messaging.interfaces;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<CorrelationWorker>();
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>();
builder.Services.AddSingleton<IRabbitMQConsumer, RabbitMQConsumer>();

var host = builder.Build();

host.Run();
