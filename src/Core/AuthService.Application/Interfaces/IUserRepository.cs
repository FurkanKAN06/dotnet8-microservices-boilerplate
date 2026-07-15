using System.Threading;
using System.Threading.Tasks;
using AuthService.Domain.Entities;
using BuildingBlocks.Application.Interfaces;

namespace AuthService.Application.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    }
}
