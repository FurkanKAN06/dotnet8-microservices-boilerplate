using BuildingBlocks.Domain.Common;
using System;

namespace AuthService.Domain.Entities
{
    public class RefreshTokenEntity : BaseEntity
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
    }
}
