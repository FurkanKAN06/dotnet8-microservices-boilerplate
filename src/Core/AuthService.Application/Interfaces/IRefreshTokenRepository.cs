using System.Threading;
using System.Threading.Tasks;
using AuthService.Domain.Entities;
using BuildingBlocks.Application.Interfaces;

namespace AuthService.Application.Interfaces
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshTokenEntity>
    {
        Task<RefreshTokenEntity?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    }
}
