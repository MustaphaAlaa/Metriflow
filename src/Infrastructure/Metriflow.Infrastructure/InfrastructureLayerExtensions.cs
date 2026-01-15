using IRepository.Generic;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Generic;

namespace Infrastructure.Extensions;

public static class InfrastructureLayerExtensions
{
    public static void AddInfrastructureLayer(this IServiceCollection service)
    {
        service.AddScoped<IPageRepository, PageRepository>();
        service.AddScoped<IDailyAnalyticsRepository, DailyAnalyticsRepository>();
        service.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        service.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
    }
}
