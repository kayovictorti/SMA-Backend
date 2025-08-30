using SMA.Application;     // sua camada Application
using SMA.Infrastructure; // sua camada Infrastructure

var builder = WebApplication.CreateBuilder(args);

// ?? 1. Configura��es principais
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Registro das depend�ncias das suas camadas
builder.Services.AddInfrastructureDI(builder.Configuration);
builder.Services.AddApplicationDI();

var app = builder.Build();

//Pipeline de execu��o
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

//Mapear controllers automaticamente
app.MapControllers();

app.Run();
