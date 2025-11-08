using Metriflow.Application;
using Metriflow.Application.Entities;
using Metriflow.Application.interfaces;
using Metriflow.Producers.Implementation;
using Metriflow.Producers.Interfaces;
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
                    services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>();
                    services.AddSingleton<IRabbitMQProducer, RabbitMQProducer>();
                    services.AddScoped<ISeedData, SeedData>();
                    services.AddScoped<IProducer, Producer>();
                }
            )
            .Build();

        await host.RunAsync();
    }
}
