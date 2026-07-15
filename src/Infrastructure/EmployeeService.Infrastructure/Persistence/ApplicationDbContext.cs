using Microsoft.EntityFrameworkCore;
using EmployeeService.Domain.Entities;
using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Infrastructure.Extensions;
using System.Threading;
using System.Threading.Tasks;

namespace EmployeeService.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IUnitOfWork
    {
        public DbSet<Employee> Employees { get; set; } = null!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employees");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.IdentityNumber).HasMaxLength(11).IsRequired();
                entity.Property(e => e.PhoneNumber).HasMaxLength(15).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            });

            modelBuilder.ApplyGlobalQueryFilter();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
