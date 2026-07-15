using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using BuildingBlocks.Extensions.Vault;
using System.Text;

namespace BuildingBlocks.Extensions
{
    public static class AuthExtension
    {
        public static IServiceCollection AddCustomJwtAuthentication(this IServiceCollection services, JwtSecrets jwtSecrets)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(jwtSecrets.SecretKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSecrets.Issuer,
                    ValidAudience = jwtSecrets.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
                };
            });

            services.AddAuthorization();

            return services;
        }
    }
}
