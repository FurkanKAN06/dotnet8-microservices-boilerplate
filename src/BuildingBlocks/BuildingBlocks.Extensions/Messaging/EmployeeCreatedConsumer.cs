using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BuildingBlocks.Extensions.Messaging
{
    public class EmployeeCreatedConsumer : IConsumer<EmployeeCreatedIntegrationEvent>
    {
        private readonly ILogger<EmployeeCreatedConsumer> _logger;

        public EmployeeCreatedConsumer(ILogger<EmployeeCreatedConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<EmployeeCreatedIntegrationEvent> context)
        {
            EmployeeCreatedIntegrationEvent message = context.Message;

            _logger.LogInformation(
                "EmployeeCreated event consumed: EmployeeId={EmployeeId}, Name={FirstName} {LastName}",
                message.EmployeeId,
                message.FirstName,
                message.LastName);

            return Task.CompletedTask;
        }
    }
}
