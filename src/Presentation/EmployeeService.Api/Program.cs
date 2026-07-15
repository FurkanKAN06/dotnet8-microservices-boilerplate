using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using Serilog.Sinks.Graylog;
using System;
using System.Threading.RateLimiting;
using BuildingBlocks.Extensions;
using BuildingBlocks.Extensions.Vault;
using FluentValidation;
using MediatR;
using BuildingBlocks.Application.Behaviors;
using BuildingBlocks.Presentation.Middleware;
using EmployeeService.Infrastructure;
using EmployeeService.Infrastructure.Persistence;

namespace EmployeeService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);


            VaultConnectionSettings vaultSettings = new VaultConnectionSettings();
            builder.Configuration.GetSection("Vault").Bind(vaultSettings);

            VaultSecrets secrets = VaultSecretLoader.LoadSecretsAsync(vaultSettings).GetAwaiter().GetResult();


            builder.Services.AddSingleton<VaultSecrets>(secrets);
            builder.Services.AddSingleton<JwtSecrets>(secrets.Jwt);


            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Graylog(new GraylogSinkOptions
                {
                    HostnameOrAddress = secrets.Graylog.Host,
                    Port = secrets.Graylog.Port
                })
                .CreateLogger();

            builder.Host.UseSerilog();


            builder.Services.AddInfrastructureServices(secrets.Database.ToConnectionString());


            builder.Services.AddHealthChecks()
                .AddNpgSql(
                    secrets.Database.ToConnectionString(),
                    name: "Database_Check",
                    failureStatus: HealthStatus.Unhealthy)
                .AddRabbitMQ(
                    rabbitConnectionString: secrets.RabbitMq.ToAmqpConnectionString(),
                    name: "RabbitMQ_Check",
                    failureStatus: HealthStatus.Unhealthy)
                .AddUrlGroup(
                    new Uri(secrets.VaultHealth.HealthUrl),
                    name: "Vault_Check",
                    failureStatus: HealthStatus.Unhealthy)
                .AddUrlGroup(
                    new Uri(secrets.Graylog.HealthUrl),
                    name: "Graylog_Check",
                    failureStatus: HealthStatus.Unhealthy);


            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(EmployeeService.Application.Features.Employees.Commands.CreateEmployee.CreateEmployeeCommand).Assembly);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(BuildingBlocks.Application.Behaviors.CachingBehavior<,>));
                cfg.AddOpenBehavior(typeof(KvkkCrudLoggingBehavior<,>));
            });


            builder.Services.AddValidatorsFromAssembly(typeof(EmployeeService.Application.Features.Employees.Commands.CreateEmployee.CreateEmployeeCommand).Assembly);
            builder.Services.AddDistributedMemoryCache();


            builder.Services.AddCustomMassTransit<EmployeeService.Infrastructure.Persistence.ApplicationDbContext>(secrets);
            builder.Services.AddCustomOpenTelemetry(secrets, "EmployeeService");


            builder.Services.AddCustomJwtAuthentication(secrets.Jwt);


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("DefaultCorsPolicy", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });


            builder.Services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<Microsoft.AspNetCore.Http.HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                options.RejectionStatusCode = 429;
            });


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "EmployeeService API",
                    Version = "v1",
                    Description = "Enterprise .NET 8 Microservice Boilerplate"
                });

                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "JWT token'ınızı girin. Örnek: eyJhbGciOiJIUzI1..."
                });

                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        System.Array.Empty<string>()
                    }
                });
            });

            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new Asp.Versioning.HeaderApiVersionReader("x-api-version");
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            builder.Services.AddControllers();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<BuildingBlocks.Application.Interfaces.Security.ICurrentUserService, BuildingBlocks.Presentation.Services.CurrentUserService>();

            WebApplication app = builder.Build();


            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Employee Service API V1");
                    c.RoutePrefix = "swagger";
                });
            }

            app.UseExceptionHandler();

            app.UseRateLimiter();

            app.UseCors("DefaultCorsPolicy");

            app.UseMiddleware<BuildingBlocks.Presentation.Middleware.TraceIdMiddleware>();
            app.UseMiddleware<BuildingBlocks.Presentation.Middleware.IdempotencyMiddleware>();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.MapControllers();

            app.Run();
        }
    }
}
