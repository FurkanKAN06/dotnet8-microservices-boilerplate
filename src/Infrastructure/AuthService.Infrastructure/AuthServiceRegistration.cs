using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AuthService.Application.Interfaces;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Services;
using BuildingBlocks.Application.Interfaces;

namespace AuthService.Infrastructure
{
    public static class AuthServiceRegistration
    {
        public static IServiceCollection AddAuthServices(this IServiceCollection services, string connectionString)
        {
            services.AddScoped<BuildingBlocks.Infrastructure.Interceptors.AuditableEntityInterceptor>();

            services.AddDbContext<AuthDbContext>((sp, options) =>
            {
                BuildingBlocks.Infrastructure.Interceptors.AuditableEntityInterceptor interceptor = sp.GetRequiredService<BuildingBlocks.Infrastructure.Interceptors.AuditableEntityInterceptor>();
                options.UseNpgsql(connectionString)
                       .AddInterceptors(interceptor);
            });

            services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AuthDbContext>());
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IJwtProvider, JwtProvider>();

            return services;
        }

        public static IServiceCollection AddAuthApplicationServices(this IServiceCollection services)
        {
            return services;
        }
    }
}

