﻿using Microsoft.EntityFrameworkCore;
using SMA.Application;
using SMA.Infrastructure;
using SMA.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Configurações principais
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// AutoMapper para Application e API
builder.Services.AddAutoMapper(
    typeof(SMA.Application.Mappings.ApplicationProfile).Assembly,
    typeof(SMA.Api.Mappings.ApiProfile).Assembly
);

// Dependências das camadas
builder.Services.AddInfrastructureDI(builder.Configuration);
builder.Services.AddApplicationDI();

// Pipeline de execução da API
var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Executa migrations automaticamente ao iniciar a aplicação
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();

