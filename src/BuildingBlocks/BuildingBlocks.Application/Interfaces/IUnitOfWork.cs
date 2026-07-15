using System.Threading;
using System.Threading.Tasks;

namespace BuildingBlocks.Application.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
