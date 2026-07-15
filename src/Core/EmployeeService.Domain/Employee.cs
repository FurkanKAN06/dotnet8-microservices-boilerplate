using BuildingBlocks.Domain.Common;
using EmployeeService.Domain.Events;
using EmployeeService.Domain.ValueObjects;

namespace EmployeeService.Domain.Entities
{
    public class Employee : AggregateRoot
    {
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string IdentityNumber { get; private set; } = string.Empty;
        public string PhoneNumber { get; private set; } = string.Empty;

        private Employee() { }

        public static Employee Create(string firstName, string lastName, string identityNumber, string phoneNumber, string? createdBy = null)
        {
            ValueObjects.IdentityNumber validatedIdentity = ValueObjects.IdentityNumber.Create(identityNumber);

            Employee employee = new Employee
            {
                FirstName = firstName,
                LastName = lastName,
                IdentityNumber = validatedIdentity.Value,
                PhoneNumber = phoneNumber,
                CreatedBy = createdBy
            };

            employee.AddDomainEvent(new EmployeeCreatedEvent(employee.Id, firstName, lastName));

            return employee;
        }

        public void Update(string firstName, string lastName, string phoneNumber, string? updatedBy = null)
        {
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
            MarkAsUpdated(updatedBy);
        }
    }
}
