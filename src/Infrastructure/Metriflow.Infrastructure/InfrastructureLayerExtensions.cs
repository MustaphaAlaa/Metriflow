using IRepository.Generic;
using Metriflow.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Generic;

namespace Infrastructure.Extensions;

public static class InfrastructureLayerExtensions
{
    public static void AddInfrastructureLayer(this IServiceCollection service, IConfigurationManager configuration)
    { 
        
        var conn = configuration.GetConnectionString("Postgres")
                   ?? throw new InvalidOperationException("Postgres connection string not found");
        Console.WriteLine($"Connection string :{conn?? "not found there a null"}");
        service.AddDbContext<MetriflowDbContext>(
            options => options.UseNpgsql( conn),
            ServiceLifetime.Scoped
        );

        service.AddScoped<IPageRepository, PageRepository>();
         service.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        
    }
}
