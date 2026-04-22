using IRepository;
using IRepository.Generic;
using Metriflow.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Ado;
using Repositories.Generic;

namespace Infrastructure.Extensions;

public static class InfrastructureLayerExtensions
{
    public static void AddInfrastructureLayer(this IServiceCollection service, IConfigurationManager configuration)
    {
        var conn = configuration.GetConnectionString("MSSQL")
                   ?? throw new InvalidOperationException("SQLServer connection string not found");
        Console.WriteLine($"Connection string :{conn ?? "not found there a null"}");
        service.AddDbContext<MetriflowDbContext>(
            options => options.UseSqlServer(conn),
            ServiceLifetime.Scoped
        );

        service.AddScoped<IPageRepository, PageRepository>();
        service.AddScoped<IAggregationProgressRepository, AggregationProgressRepository>();
        service.AddScoped<ITrackTableCountRepository, TrackTableCountRepository>();
        service.AddScoped<IUow, UnitOfWork>();

        service.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
    }
}