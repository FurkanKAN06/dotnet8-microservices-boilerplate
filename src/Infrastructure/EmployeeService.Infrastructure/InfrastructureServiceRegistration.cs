using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BuildingBlocks.Application.Interfaces;
using EmployeeService.Application.Interfaces;
using EmployeeService.Infrastructure.Persistence;
using EmployeeService.Infrastructure.Repositories;

namespace EmployeeService.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
        {
            services.AddScoped<BuildingBlocks.Infrastructure.Interceptors.AuditableEntityInterceptor>();

            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                BuildingBlocks.Infrastructure.Interceptors.AuditableEntityInterceptor interceptor = sp.GetRequiredService<BuildingBlocks.Infrastructure.Interceptors.AuditableEntityInterceptor>();
                options.UseNpgsql(connectionString)
                       .AddInterceptors(interceptor);
            });

            services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();

            return services;
        }
    }
}
