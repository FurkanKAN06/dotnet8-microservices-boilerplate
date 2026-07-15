using EmployeeService.Application.Interfaces;
using EmployeeService.Domain.Entities;
using BuildingBlocks.Infrastructure.Persistence;

namespace EmployeeService.Infrastructure.Repositories
{
    public class EmployeeRepository : EfCoreRepositoryBase<Employee, Persistence.ApplicationDbContext>, IEmployeeRepository
    {
        public EmployeeRepository(Persistence.ApplicationDbContext context) : base(context)
        {
        }
    }
}
