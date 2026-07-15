using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using BuildingBlocks.Domain.Common;
using BuildingBlocks.Application.Interfaces.Security;
using System.Threading;
using System.Threading.Tasks;

namespace BuildingBlocks.Infrastructure.Interceptors
{
    public class AuditableEntityInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUserService;

        public AuditableEntityInterceptor(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateEntities(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            UpdateEntities(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateEntities(DbContext? context)
        {
            if (context == null) return;

            foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<BaseEntity> entry in context.ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    if (string.IsNullOrEmpty(entry.Entity.CreatedBy))
                    {
                        entry.Entity.GetType().GetProperty("CreatedBy")?.SetValue(entry.Entity, _currentUserService.UserName ?? "System");
                    }
                }
                else if (entry.State == EntityState.Modified || entry.HasChangedOwnedEntities())
                {
                    entry.Entity.MarkAsUpdated(_currentUserService.UserName ?? "System");
                }
                else if (entry.State == EntityState.Deleted)
                {
                }
            }
        }
    }

    public static class ChangeTrackerExtensions
    {
        public static bool HasChangedOwnedEntities(this Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry) =>
            entry.References.Any(r => 
                r.TargetEntry != null && 
                r.TargetEntry.Metadata.IsOwned() && 
                (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
    }
}
