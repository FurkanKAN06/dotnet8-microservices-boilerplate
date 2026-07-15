using BuildingBlocks.Domain.Models;
using BuildingBlocks.Domain.Models.Pagination;
using EmployeeService.Application.DTOs;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Queries.GetEmployees
{
    public class GetEmployeesQuery : IRequest<Result<PagedResult<EmployeeDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
    }
}
