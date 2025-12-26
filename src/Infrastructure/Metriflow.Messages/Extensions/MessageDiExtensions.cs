using Metriflow.Application;
using Metriflow.Application.Entities;
using Metriflow.Application.interfaces;
using Metriflow.Application.Interfaces.Workers;
using Metriflow.Messages.Producers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Metriflow.Messages.Extensions;


public static class MessageDiExtensions
{
    public static IServiceCollection AddRabbitMqDi(this IServiceCollection services)
    {
        services.AddSingleton<IMessageBrokerConsumer, RabbitMqConsumer>();
        services.AddSingleton<IMessageBrokerProducer, RabbitMqProducer>();
        return services;
    }
} 