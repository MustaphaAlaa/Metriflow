
using Metriflow.Application.interfaces;
using Metriflow.Application.Interfaces.Workers;
using Metriflow.Application.Services;
using Metriflow.Application.Services.Workers;
using Microsoft.Extensions.DependencyInjection;

namespace Metriflow.Application.Extensions;

public static class ApplicationLayerExtensions
{
    public static IServiceCollection AddApplicationLayerDiServices(this IServiceCollection service)
    {
        service.AddScoped<IPageServices, PageServices>();
        service.AddScoped<IRawDataServices, RawDataServices>();
        service.AddScoped<IDailyStatCalculator, DailyStateCalculator>();

        return service;

    }
    public static IServiceCollection AddApplicationLayerDiMessagesServices(this IServiceCollection service)
    {
        service.AddScoped<IProducer, Producer>();

        return service;

    }
    public static IServiceCollection AddApplicationLayerDiJsonReader(this IServiceCollection service)
    {
        service.AddScoped<IStreamData, StreamData>();
        return service;

    }
    
}
