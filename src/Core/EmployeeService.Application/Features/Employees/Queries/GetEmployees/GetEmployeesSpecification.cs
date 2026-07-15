using BuildingBlocks.Application.Specifications;
using EmployeeService.Domain.Entities;

namespace EmployeeService.Application.Features.Employees.Queries.GetEmployees
{
    public class GetEmployeesSpecification : BaseSpecification<Employee>
    {
        public GetEmployeesSpecification(GetEmployeesQuery request)
            : base(x => string.IsNullOrEmpty(request.SearchTerm) || 
                        x.FirstName.Contains(request.SearchTerm) || 
                        x.LastName.Contains(request.SearchTerm))
        {
            ApplyOrderByDescending(x => x.CreatedAt);
            ApplyPaging((request.PageNumber - 1) * request.PageSize, request.PageSize);
        }
    }
}
