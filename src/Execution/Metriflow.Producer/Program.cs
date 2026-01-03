using Metriflow.Application;
using Metriflow.Application.Entities;
using Metriflow.Application.Extensions;
using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Workers;
using Metriflow.Application.Services;
using Metriflow.Messages.Connections;
using Metriflow.Messages.Extensions;
using Metriflow.Producers.Implementation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Metriflow.Producers;

internal class Program
{
    private static async Task Main(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build(); 

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(
                (hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;
                     services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMQ"));
                    services.AddHostedService<MessageProducer>();
                     services.AddSingleton<IMessageBrokerConnection, RabbitMqConnection>();
                    services.AddRabbitMqDi( );
                    
                    services.AddRegisterReflection();
                    ;
                }
            )
            .Build();

        await host.RunAsync();
        
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
        System.Console.WriteLine("I'm done");
    }
}



