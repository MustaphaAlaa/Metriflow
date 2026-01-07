using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Workers;
using Metriflow.Application.Services;
using Metriflow.Application.Services.Workers;
using Metriflow.Application.Worker;
using Metriflow.Domain.Interfaces;
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