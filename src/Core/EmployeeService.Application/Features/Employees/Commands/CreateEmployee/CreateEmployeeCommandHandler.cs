using MediatR;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Domain.Models;
using BuildingBlocks.Application.Interfaces;
using EmployeeService.Application.DTOs;
using EmployeeService.Domain.Entities;
using EmployeeService.Application.Interfaces;
using EmployeeService.Application.Mappers;

namespace EmployeeService.Application.Features.Employees.Commands.CreateEmployee
{
    public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Result<EmployeeDto>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateEmployeeCommandHandler(IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork)
        {
            _employeeRepository = employeeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<EmployeeDto>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
        {
            Employee employee = Employee.Create(request.FirstName, request.LastName, request.IdentityNumber, request.PhoneNumber);

            await _employeeRepository.AddAsync(employee, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            EmployeeMapper mapper = new EmployeeMapper();
            EmployeeDto dto = mapper.EmployeeToEmployeeDto(employee);

            return Result<EmployeeDto>.Success(dto);
        }
    }
}

