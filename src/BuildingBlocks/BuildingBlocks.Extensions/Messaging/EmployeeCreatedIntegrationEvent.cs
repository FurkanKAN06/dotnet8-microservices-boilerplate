using System;

namespace BuildingBlocks.Extensions.Messaging
{
    public class EmployeeCreatedIntegrationEvent
    {
        public int EmployeeId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
