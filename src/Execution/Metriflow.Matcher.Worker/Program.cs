using Metriflow.Application.Entities;
using Metriflow.Application.Extensions;
using Metriflow.Application.Interfaces;
using Metriflow.Application.Services.Workers;
using Metriflow.Correlation.Worker;
using Metriflow.Matcher.Worker;
using Metriflow.Messages.Connections;
using Metriflow.Messages.Extensions;
using Metriflow.Redis.Extensions;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<MatcherWorker>();


builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.Configure<AnalyticsOptions>(builder.Configuration.GetSection("Analytics"));


builder.Services.AddSingleton<IMessageBrokerConnection, RabbitMqConnection>(); 
builder.Services.AddRedisDI(builder.Configuration);
builder.Services.AddApplicationLayerDiMessagesServices();
builder.Services.AddRabbitMqDi();


 
var host = builder.Build();

host.Run();