using IRepository.Generic;
using Metriflow.Application.Extensions;
using Metriflow.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Repositories.Generic;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MetriflowDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("Sqlserver")),
    ServiceLifetime.Scoped
);

builder.Services.AddApplicationLayer();

// builder.Services.AddScoped<IPageServices, PageServices>();
// builder.Services.AddScoped<IRawDataServices, RawDataServices>();
// builder.Services.AddScoped<IDailyStateServices, DailyStateServices>();

builder.Services.AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>));
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MetriflowDbContext>();
    dbContext.Database.Migrate(); // This ensures migrations are applied
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
