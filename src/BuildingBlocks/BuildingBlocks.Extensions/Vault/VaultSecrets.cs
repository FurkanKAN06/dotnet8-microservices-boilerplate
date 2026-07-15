namespace BuildingBlocks.Extensions.Vault
{
    public class VaultSecrets
    {
        public DatabaseSecrets Database { get; set; } = new DatabaseSecrets();
        public RabbitMqSecrets RabbitMq { get; set; } = new RabbitMqSecrets();
        public GraylogSecrets Graylog { get; set; } = new GraylogSecrets();
        public JaegerSecrets Jaeger { get; set; } = new JaegerSecrets();
        public JwtSecrets Jwt { get; set; } = new JwtSecrets();
        public VaultHealthSecrets VaultHealth { get; set; } = new VaultHealthSecrets();
    }

    public class DatabaseSecrets
    {
        public string Host { get; set; } = string.Empty;
        public string Port { get; set; } = "5432";
        public string Database { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string ToConnectionString()
        {
            return $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password}";
        }
    }

    public class RabbitMqSecrets
    {
        public string Host { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Port { get; set; } = 5672;

        public string ToAmqpConnectionString()
        {
            return $"amqp://{Username}:{Password}@{Host}:{Port}";
        }
    }

    public class GraylogSecrets
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 12201;
        public string HealthUrl { get; set; } = string.Empty;
    }

    public class JaegerSecrets
    {
        public string Endpoint { get; set; } = string.Empty;
    }

    public class JwtSecrets
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int AccessTokenExpirationMinutes { get; set; } = 60;
        public int RefreshTokenExpirationDays { get; set; } = 7;
    }

    public class VaultHealthSecrets
    {
        public string HealthUrl { get; set; } = string.Empty;
    }
}
