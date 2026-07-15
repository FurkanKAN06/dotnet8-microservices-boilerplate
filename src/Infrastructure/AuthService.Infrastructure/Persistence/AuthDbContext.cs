using Microsoft.EntityFrameworkCore;
using AuthService.Domain.Entities;
using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Infrastructure.Extensions;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.Persistence
{
    public class AuthDbContext : DbContext, IUnitOfWork
    {
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }

        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<RefreshTokenEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Token).IsUnique();
            });

            modelBuilder.ApplyGlobalQueryFilter();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}

