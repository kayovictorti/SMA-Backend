using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SMA.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureDI(this IServiceCollection services, IConfiguration config)
        {
            return services;
        }
    }
}
