using BuildingBlocks.Domain.Models;
using BuildingBlocks.Domain.Models.Pagination;
using EmployeeService.Application.DTOs;
using EmployeeService.Application.Interfaces;
using EmployeeService.Application.Mappers;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EmployeeService.Application.Features.Employees.Queries.GetEmployees
{
    public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, Result<PagedResult<EmployeeDto>>>
    {
        private readonly IEmployeeRepository _repository;
        
        public GetEmployeesQueryHandler(IEmployeeRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<PagedResult<EmployeeDto>>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
        {
            GetEmployeesSpecification spec = new GetEmployeesSpecification(request);
            
            System.Collections.Generic.IReadOnlyList<EmployeeService.Domain.Entities.Employee> employees = await _repository.ListAsync(spec, cancellationToken);
            int totalCount = await _repository.CountAsync(spec, cancellationToken);
            
            EmployeeMapper mapper = new EmployeeMapper();
            System.Collections.Generic.List<EmployeeDto> dtos = employees.Select(e => mapper.EmployeeToEmployeeDto(e)).ToList();

            PagedResult<EmployeeDto> pagedResult = new PagedResult<EmployeeDto>(dtos, totalCount, request.PageNumber, request.PageSize);
            
            return Result<PagedResult<EmployeeDto>>.Success(pagedResult);
        }
    }
}
