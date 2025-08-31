using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SMA.Application.Interfaces;
using SMA.Application.Services;
using SMA.Infrastructure.Integrations;
using SMA.Infrastructure.Persistence;
using SMA.Infrastructure.Repositories;

namespace SMA.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureDI(this IServiceCollection services, IConfiguration cfg)
        {
            var cs = cfg.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada.");

            var csb = new SqliteConnectionStringBuilder(cs);
            var relative = csb.DataSource;
            var fullPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relative));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            var finalCs = new SqliteConnectionStringBuilder { DataSource = fullPath }.ToString();

            services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(finalCs));

            services.Configure<IotOptions>(cfg.GetSection("Iot"));
            services.AddHttpClient<IotIntegrationClient>()
                .ConfigureHttpClient((sp, client) =>
                {
                    var opts = sp.GetRequiredService<IOptions<IotOptions>>().Value;
                    if (!string.IsNullOrWhiteSpace(opts.BaseUrl))
                        client.BaseAddress = new Uri(opts.BaseUrl);
                });

            services.AddScoped<IIotIntegrationClient, IotIntegrationClient>();
            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IDeviceRepository, DeviceRepository>();
            services.AddScoped<IEventRepository, EventRepository>();

            return services;
        }
    }
}
