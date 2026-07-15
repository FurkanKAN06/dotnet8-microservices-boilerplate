using BuildingBlocks.Domain.Models;
using EmployeeService.Application.DTOs;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands.CreateEmployee
{
    public class CreateEmployeeCommand : IRequest<Result<EmployeeDto>>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
