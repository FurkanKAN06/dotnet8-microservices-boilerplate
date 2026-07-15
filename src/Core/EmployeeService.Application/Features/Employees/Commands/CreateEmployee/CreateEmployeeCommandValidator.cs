using FluentValidation;

namespace EmployeeService.Application.Features.Employees.Commands.CreateEmployee
{
    public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
    {
        public CreateEmployeeCommandValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.IdentityNumber).NotEmpty().Length(11).Matches(@"^\d{11}$");
            RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(15);
        }
    }
}
