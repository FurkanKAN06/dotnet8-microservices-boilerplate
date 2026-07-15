using System;
using MediatR;

namespace BuildingBlocks.Domain.Common
{
    public interface IDomainEvent : INotification
    {
        DateTime OccurredOn { get; }
    }
}

