using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using AuthService.Domain.Entities;
using AuthService.Application.Interfaces;
using BuildingBlocks.Infrastructure.Persistence;

namespace AuthService.Infrastructure.Repositories
{
    public class UserRepository : EfCoreRepositoryBase<User, Persistence.AuthDbContext>, IUserRepository
    {
        public UserRepository(Persistence.AuthDbContext context) : base(context) { }

        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await Context.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
        }
    }
}
