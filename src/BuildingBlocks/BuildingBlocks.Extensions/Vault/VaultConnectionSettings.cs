namespace BuildingBlocks.Extensions.Vault
{
    public class VaultConnectionSettings
    {
        public string Address { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string MountPoint { get; set; } = "secret";
        public string SecretPath { get; set; } = "microservices";
    }
}
