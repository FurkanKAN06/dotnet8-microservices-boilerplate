using System;
using BuildingBlocks.Domain.Common;

namespace EmployeeService.Domain.Events
{
    public class EmployeeCreatedEvent : IDomainEvent
    {
        public int EmployeeId { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public DateTime OccurredOn { get; }

        public EmployeeCreatedEvent(int employeeId, string firstName, string lastName)
        {
            EmployeeId = employeeId;
            FirstName = firstName;
            LastName = lastName;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
