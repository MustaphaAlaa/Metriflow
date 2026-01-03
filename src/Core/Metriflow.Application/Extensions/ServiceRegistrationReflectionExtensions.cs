using System.Reflection;
using Metriflow.Domain.CustomAttributes;
using Microsoft.Extensions.DependencyInjection;

namespace Metriflow.Application.Extensions;

public static class ServiceRegistrationReflectionExtensions
{
    public static IServiceCollection AddRegisterReflection(this IServiceCollection service)
    {
        var typesWithAttribute = AppDomain.CurrentDomain.GetAssemblies()
            //.Where(assembly => !assembly.IsDynamic && assembly.FullName!.StartsWith("Metriflow"))
            .SelectMany(assembly =>
                assembly.GetTypes().Where(t => t.IsDefined(typeof(ServiceRegistrationAttribute))));


        foreach (var type in typesWithAttribute)
        {
            var attribute = type.GetCustomAttribute<ServiceRegistrationAttribute>();
            if(attribute == null) continue;
            var serviceType = attribute.ServiceType ?? type.GetInterfaces().FirstOrDefault() ?? type;

            switch (attribute.LifeTime)
            {
                case ServiceLifetime.Transient:
                    service.AddTransient(serviceType,type);
                    break;
                case ServiceLifetime.Scoped:
                    service.AddScoped(serviceType,type);
                    break;
                case ServiceLifetime.Singleton:
                    service.AddSingleton(serviceType,type);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            } 
        } 
        return service;
    }
}