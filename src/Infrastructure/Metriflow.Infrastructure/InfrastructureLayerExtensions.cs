using IRepository.Generic;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Generic;

namespace Infrastructure.Extensions;

public static class InfrastructureLayerExtensions
{
    public static void AddInfrastructureLayer(this IServiceCollection service)
    {
        service.AddScoped<IPageRepository, PageRepository>();
        service.AddScoped<IDailyStatRepository, DailyStatRepository>();
        service.AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        service.AddTransient(typeof(IUnitOfWork), typeof(UnitOfWork));
    }
}
