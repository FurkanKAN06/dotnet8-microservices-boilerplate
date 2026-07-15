using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using BuildingBlocks.Extensions.Vault;
using BuildingBlocks.Extensions.Messaging;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Extensions
{
    public static class MassTransitExtension
    {
        public static IServiceCollection AddCustomMassTransit<TDbContext>(this IServiceCollection services, VaultSecrets secrets)
            where TDbContext : DbContext
        {
            services.AddMassTransit(x =>
            {
                x.AddEntityFrameworkOutbox<TDbContext>(o =>
                {
                    o.UsePostgres();
                    o.UseBusOutbox();
                });

                x.AddConsumer<EmployeeCreatedConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(secrets.RabbitMq.Host, "/", h =>
                    {
                        h.Username(secrets.RabbitMq.Username);
                        h.Password(secrets.RabbitMq.Password);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}
