using Metriflow.Application.interfaces;
using Metriflow.Application.Services; 
using Microsoft.Extensions.DependencyInjection;

namespace Metriflow.Application.Extensions;

public static class ApplicationLayerExtensions
{
    public static void AddApplicationLayer(this IServiceCollection service)
    {
        service.AddScoped<IPageServices, PageServices>();
        service.AddScoped<IDailyStateServices, DailyStateServiceses>();
        service.AddScoped<IRawDataServices, RawDataServices>();
    }
}