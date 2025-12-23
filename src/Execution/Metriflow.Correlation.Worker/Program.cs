using Metriflow.Application;
using Metriflow.Application.Entities;
using Metriflow.Application.interfaces;
using Metriflow.Correlation.Worker;
using Metriflow.Correlation.Worker.Interfaces;
using RabbitMQ.Client;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<CorrelationWorker>();
builder.Services.AddHostedService<MatcherAndProducerWorker>();
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>();

builder.Services.AddSingleton<IRabbitMQConsumer, RabbitMQConsumer>();
builder.Services.AddSingleton<IRabbitMQProducer, RabbitMQProducer>();
builder.Services.AddScoped<IConsumerMessageHandler, ConsumerMessageHandler>();
builder.Services.AddScoped<IRowDataProducer, RawDataProducer>();

builder.Services.AddScoped<ICorrelationConsumer, CorrelationConsumer>();
builder.Services.AddScoped<IRecordsMatcher, RecordsMatcher>();
builder.Services.AddScoped<IRedisQueriesCorrelation, RedisQueriesCorrelation>();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisConnection = builder.Configuration.GetConnectionString("Redis");
    var options = ConfigurationOptions.Parse(redisConnection);
    options.AbortOnConnectFail = true;
    return ConnectionMultiplexer.Connect(options);
});
var host = builder.Build();

host.Run();
