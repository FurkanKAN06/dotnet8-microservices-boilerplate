using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Application.Interfaces
{
    public interface IJwtProvider
    {
        Task<DTOs.TokenResponseDto> GenerateTokensAsync(Domain.Entities.User user, CancellationToken cancellationToken);
    }
}
