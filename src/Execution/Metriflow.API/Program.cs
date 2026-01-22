using Infrastructure.Extensions;
using IRepository.Generic;
using Metriflow.Application.Extensions;
using Metriflow.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Repositories.Generic;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddApplicationLayerDiServices();
builder.Services.AddInfrastructureLayer(builder.Configuration);

var app = builder.Build();

// using (var scope = app.Services.CreateScope())
// {
//     var dbContext = scope.ServiceProvider.GetRequiredService<MetriflowDbContext>();
//     dbContext.Database.Migrate();
// }

 
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
