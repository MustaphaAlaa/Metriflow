using Metriflow.Correlation.Worker;
using Metriflow.Correlation.Worker.Interfaces;
using Metriflow.Messaging;
using Metriflow.Messaging.Entities;
using Metriflow.Messaging.interfaces;
using RabbitMQ.Client;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<CorrelationWorker>();
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>();

builder.Services.AddScoped<IRabbitMQConsumer, RabbitMQConsumer>();
builder.Services.AddScoped<IRabbitMQProducer, RabbitMQProducer>();

builder.Services.AddScoped<IConsumerMessageHandler, ConsumerMessageHandler>();
builder.Services.AddScoped<IRowRecordProducer, RawRecordProducer>();

builder.Services.AddScoped<IConsumer, Consumer>();
builder.Services.AddScoped<ICombiner, Combiner>();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisConnection = builder.Configuration.GetConnectionString("Redis");
    var options = ConfigurationOptions.Parse(redisConnection);
    options.AbortOnConnectFail = true;
    return ConnectionMultiplexer.Connect(options);
});
var host = builder.Build();

host.Run();
