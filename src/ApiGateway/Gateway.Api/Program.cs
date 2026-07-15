using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using BuildingBlocks.Extensions.Vault;
using System.Text;

namespace Gateway.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);


            VaultConnectionSettings vaultSettings = new VaultConnectionSettings();
            builder.Configuration.GetSection("Vault").Bind(vaultSettings);

            VaultSecrets secrets = VaultSecretLoader.LoadSecretsAsync(vaultSettings).GetAwaiter().GetResult();


            builder.Services.AddReverseProxy()
                .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

            builder.Services.AddHealthChecksUI().AddInMemoryStorage();


            byte[] keyBytes = Encoding.UTF8.GetBytes(secrets.Jwt.SecretKey);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = secrets.Jwt.Issuer,
                        ValidAudience = secrets.Jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("EmployeeOrAdmin", policy => policy.RequireRole("Employee", "Admin"));
            });

            WebApplication app = builder.Build();

            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecksUI(setup =>
                {
                    setup.UIPath = "/health-ui";
                });
            });

            app.MapReverseProxy();

            app.Run();
        }
    }
}
