using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;

namespace BuildingBlocks.Extensions.Vault
{
    public static class VaultSecretLoader
    {
        public static async Task<VaultSecrets> LoadSecretsAsync(VaultConnectionSettings vaultSettings)
        {
            TokenAuthMethodInfo authMethod = new TokenAuthMethodInfo(vaultSettings.Token);
            VaultClientSettings clientSettings = new VaultClientSettings(vaultSettings.Address, authMethod);
            VaultClient client = new VaultClient(clientSettings);

            VaultSecrets secrets = new VaultSecrets();

            try
            {
                VaultSharp.V1.Commons.Secret<VaultSharp.V1.Commons.SecretData> dbSecret =
                    await client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
                        path: $"{vaultSettings.SecretPath}/database",
                        mountPoint: vaultSettings.MountPoint);

                IDictionary<string, object> dbData = dbSecret.Data.Data;
                secrets.Database = new DatabaseSecrets
                {
                    Host = dbData.ContainsKey("host") ? dbData["host"]?.ToString() ?? string.Empty : string.Empty,
                    Port = dbData.ContainsKey("port") ? dbData["port"]?.ToString() ?? "5432" : "5432",
                    Database = dbData.ContainsKey("database") ? dbData["database"]?.ToString() ?? string.Empty : string.Empty,
                    Username = dbData.ContainsKey("username") ? dbData["username"]?.ToString() ?? string.Empty : string.Empty,
                    Password = dbData.ContainsKey("password") ? dbData["password"]?.ToString() ?? string.Empty : string.Empty
                };

                VaultSharp.V1.Commons.Secret<VaultSharp.V1.Commons.SecretData> rmqSecret =
                    await client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
                        path: $"{vaultSettings.SecretPath}/rabbitmq",
                        mountPoint: vaultSettings.MountPoint);

                IDictionary<string, object> rmqData = rmqSecret.Data.Data;
                secrets.RabbitMq = new RabbitMqSecrets
                {
                    Host = rmqData.ContainsKey("host") ? rmqData["host"]?.ToString() ?? string.Empty : string.Empty,
                    Username = rmqData.ContainsKey("username") ? rmqData["username"]?.ToString() ?? string.Empty : string.Empty,
                    Password = rmqData.ContainsKey("password") ? rmqData["password"]?.ToString() ?? string.Empty : string.Empty,
                    Port = rmqData.ContainsKey("port") ? int.Parse(rmqData["port"]?.ToString() ?? "5672") : 5672
                };

                VaultSharp.V1.Commons.Secret<VaultSharp.V1.Commons.SecretData> graylogSecret =
                    await client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
                        path: $"{vaultSettings.SecretPath}/graylog",
                        mountPoint: vaultSettings.MountPoint);

                IDictionary<string, object> graylogData = graylogSecret.Data.Data;
                secrets.Graylog = new GraylogSecrets
                {
                    Host = graylogData.ContainsKey("host") ? graylogData["host"]?.ToString() ?? string.Empty : string.Empty,
                    Port = graylogData.ContainsKey("port") ? int.Parse(graylogData["port"]?.ToString() ?? "12201") : 12201,
                    HealthUrl = graylogData.ContainsKey("healthUrl") ? graylogData["healthUrl"]?.ToString() ?? string.Empty : string.Empty
                };

                VaultSharp.V1.Commons.Secret<VaultSharp.V1.Commons.SecretData> jaegerSecret =
                    await client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
                        path: $"{vaultSettings.SecretPath}/jaeger",
                        mountPoint: vaultSettings.MountPoint);

                IDictionary<string, object> jaegerData = jaegerSecret.Data.Data;
                secrets.Jaeger = new JaegerSecrets
                {
                    Endpoint = jaegerData.ContainsKey("endpoint") ? jaegerData["endpoint"]?.ToString() ?? string.Empty : string.Empty
                };

                VaultSharp.V1.Commons.Secret<VaultSharp.V1.Commons.SecretData> jwtSecret =
                    await client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
                        path: $"{vaultSettings.SecretPath}/jwt",
                        mountPoint: vaultSettings.MountPoint);

                IDictionary<string, object> jwtData = jwtSecret.Data.Data;
                secrets.Jwt = new JwtSecrets
                {
                    SecretKey = jwtData.ContainsKey("secretKey") ? jwtData["secretKey"]?.ToString() ?? string.Empty : string.Empty,
                    Issuer = jwtData.ContainsKey("issuer") ? jwtData["issuer"]?.ToString() ?? string.Empty : string.Empty,
                    Audience = jwtData.ContainsKey("audience") ? jwtData["audience"]?.ToString() ?? string.Empty : string.Empty,
                    AccessTokenExpirationMinutes = jwtData.ContainsKey("accessTokenExpirationMinutes") ? int.Parse(jwtData["accessTokenExpirationMinutes"]?.ToString() ?? "60") : 60,
                    RefreshTokenExpirationDays = jwtData.ContainsKey("refreshTokenExpirationDays") ? int.Parse(jwtData["refreshTokenExpirationDays"]?.ToString() ?? "7") : 7
                };

                secrets.VaultHealth = new VaultHealthSecrets
                {
                    HealthUrl = $"{vaultSettings.Address}/v1/sys/health"
                };
            }
            catch (Exception)
            {
                throw;
            }

            return secrets;
        }
    }
}
