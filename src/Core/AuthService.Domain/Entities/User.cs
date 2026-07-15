using BuildingBlocks.Domain.Common;
using AuthService.Domain.Enums;

namespace AuthService.Domain.Entities
{
    public class User : AggregateRoot
    {
        public string Username { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public Role Role { get; private set; }

        protected User() { }

        private User(string username, string email, string passwordHash, Role role)
        {
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
        }

        public static User Create(string username, string email, string passwordHash, Role role)
        {
            return new User(username, email, passwordHash, role);
        }

        public void UpdatePassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
            MarkAsUpdated();
        }

        public void ChangeRole(Role newRole)
        {
            Role = newRole;
            MarkAsUpdated();
        }
    }
}

