using SMA.Application;
using SMA.Application.Interfaces;
using SMA.Infrastructure;
using SMA.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Configura��es principais
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// AutoMapper para Application e API
builder.Services.AddAutoMapper(
    typeof(SMA.Application.Mappings.ApplicationProfile).Assembly,
    typeof(SMA.Api.Mappings.ApiProfile).Assembly
);

// Depend�ncias das camadas
builder.Services.AddInfrastructureDI(builder.Configuration);
builder.Services.AddApplicationDI();

// Pipeline de execu��o da API
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

app.Run();
