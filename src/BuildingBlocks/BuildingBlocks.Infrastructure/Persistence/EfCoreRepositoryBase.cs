using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Domain.Common;
using BuildingBlocks.Application.Interfaces;

namespace BuildingBlocks.Infrastructure.Persistence
{
    public class EfCoreRepositoryBase<TEntity, TContext> : IGenericRepository<TEntity>
        where TEntity : BaseEntity
        where TContext : DbContext
    {
        protected readonly TContext Context;

        public EfCoreRepositoryBase(TContext context)
        {
            Context = context;
        }

        public virtual async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await Context.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public virtual async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await Context.Set<TEntity>().ToListAsync(cancellationToken);
        }

        public async Task<System.Collections.Generic.IReadOnlyList<TEntity>> ListAsync(BuildingBlocks.Application.Specifications.ISpecification<TEntity> spec, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(spec).ToListAsync(cancellationToken);
        }

        public async Task<int> CountAsync(BuildingBlocks.Application.Specifications.ISpecification<TEntity> spec, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(spec, evaluatePagination: false).CountAsync(cancellationToken);
        }

        private System.Linq.IQueryable<TEntity> ApplySpecification(BuildingBlocks.Application.Specifications.ISpecification<TEntity> spec, bool evaluatePagination = true)
        {
            return BuildingBlocks.Infrastructure.Specifications.SpecificationEvaluator<TEntity>.GetQuery(Context.Set<TEntity>().AsQueryable(), spec, evaluatePagination);
        }

        public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> entry = await Context.Set<TEntity>().AddAsync(entity, cancellationToken);
            return entry.Entity;
        }

        public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Context.Set<TEntity>().Update(entity);
            return Task.CompletedTask;
        }

        public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            entity.MarkAsDeleted();
            Context.Set<TEntity>().Update(entity);
            return Task.CompletedTask;
        }
    }
}
