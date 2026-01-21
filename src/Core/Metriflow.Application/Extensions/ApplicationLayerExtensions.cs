using Metriflow.Application.Interfaces; 
using Metriflow.Application.Services;  
using Microsoft.Extensions.DependencyInjection;

namespace Metriflow.Application.Extensions;

public static class ApplicationLayerExtensions
{
    public static IServiceCollection AddApplicationLayerDiServices(this IServiceCollection service)
    {
        service.AddScoped<IPageServices, PageServices>();
        service.AddScoped<IDailyAnalyticsService, DailyAnalyticsService>();
        return service;
    }
}