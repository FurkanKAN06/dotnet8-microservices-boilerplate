using Riok.Mapperly.Abstractions;
using EmployeeService.Domain.Entities;
using EmployeeService.Application.DTOs;

namespace EmployeeService.Application.Mappers
{
    [Mapper]
    public partial class EmployeeMapper
    {
        public partial EmployeeDto EmployeeToEmployeeDto(Employee employee);
    }
}
