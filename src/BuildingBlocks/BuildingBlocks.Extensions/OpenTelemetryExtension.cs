using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using BuildingBlocks.Extensions.Vault;
using System;

namespace BuildingBlocks.Extensions
{
    public static class OpenTelemetryExtension
    {
        public static IServiceCollection AddCustomOpenTelemetry(this IServiceCollection services, VaultSecrets secrets, string serviceName)
        {
            services.AddOpenTelemetry()
                .WithTracing(tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddOtlpExporter(opt =>
                        {
                            opt.Endpoint = new Uri(secrets.Jaeger.Endpoint);
                        });
                });

            return services;
        }
    }
}
