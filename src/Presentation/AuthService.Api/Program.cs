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
using AuthService.Infrastructure;

namespace AuthService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            VaultConnectionSettings vaultSettings = new VaultConnectionSettings();
            builder.Configuration.GetSection("Vault").Bind(vaultSettings);

            VaultSecrets vaultSecrets = VaultSecretLoader.LoadSecretsAsync(vaultSettings).GetAwaiter().GetResult();
            builder.Services.AddSingleton(vaultSecrets);
            builder.Services.AddSingleton<JwtSecrets>(vaultSecrets.Jwt);

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Graylog(new GraylogSinkOptions
                {
                    HostnameOrAddress = vaultSecrets.Graylog.Host,
                    Port = vaultSecrets.Graylog.Port
                })
                .CreateLogger();

            builder.Host.UseSerilog();

            string connectionString = $"Host={vaultSecrets.Database.Host};Port={vaultSecrets.Database.Port};Database=auth_db;Username={vaultSecrets.Database.Username};Password={vaultSecrets.Database.Password}";

            builder.Services.AddAuthServices(connectionString);
            builder.Services.AddAuthApplicationServices();

            builder.Services.AddHealthChecks()
                .AddNpgSql(
                    connectionString,
                    name: "Database_Check",
                    failureStatus: HealthStatus.Unhealthy)
                .AddRabbitMQ(
                    rabbitConnectionString: vaultSecrets.RabbitMq.ToAmqpConnectionString(),
                    name: "RabbitMQ_Check",
                    failureStatus: HealthStatus.Unhealthy);

            builder.Services.AddCustomMassTransit<AuthService.Infrastructure.Persistence.AuthDbContext>(vaultSecrets);
            builder.Services.AddCustomOpenTelemetry(vaultSecrets, "AuthService");

            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(AuthService.Application.Features.Auth.Login.LoginCommand).Assembly);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
                cfg.AddOpenBehavior(typeof(KvkkCrudLoggingBehavior<,>));
            });

            builder.Services.AddValidatorsFromAssembly(typeof(AuthService.Application.Features.Auth.Login.LoginCommand).Assembly);
            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddCustomJwtAuthentication(vaultSecrets.Jwt);

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
                    Title = "AuthService API",
                    Version = "v1",
                    Description = "Enterprise .NET 8 Auth Microservice"
                });

                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header
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
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth Service API V1");
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
