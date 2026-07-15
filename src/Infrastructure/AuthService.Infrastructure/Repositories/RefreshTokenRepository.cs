using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using AuthService.Domain.Entities;
using AuthService.Application.Interfaces;
using BuildingBlocks.Infrastructure.Persistence;

namespace AuthService.Infrastructure.Repositories
{
    public class RefreshTokenRepository : EfCoreRepositoryBase<RefreshTokenEntity, Persistence.AuthDbContext>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(Persistence.AuthDbContext context) : base(context) { }

        public async Task<RefreshTokenEntity?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await Context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token && !r.IsRevoked, cancellationToken);
        }
    }
}
