using EmployeeService.Domain.Entities;
using BuildingBlocks.Application.Interfaces;

namespace EmployeeService.Application.Interfaces
{
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
    }
}
