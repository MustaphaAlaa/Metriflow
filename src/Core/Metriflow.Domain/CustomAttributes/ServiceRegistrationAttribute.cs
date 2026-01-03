using Microsoft.Extensions.DependencyInjection;

namespace Metriflow.Domain.CustomAttributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ServiceRegistrationAttribute(ServiceLifetime lifetime = ServiceLifetime.Scoped, Type serviceType = null)
    : Attribute
{
    public ServiceLifetime LifeTime { get; } = lifetime;
    public Type ServiceType { get; } = serviceType;
} 