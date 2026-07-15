using System;

namespace BuildingBlocks.Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        public string Title { get; }

        protected DomainException(string title, string message) : base(message)
        {
            Title = title;
        }
    }

    public class NotFoundException : DomainException
    {
        public NotFoundException(string message) : base("Not Found", message)
        {
        }
    }

    public class BadRequestException : DomainException
    {
        public BadRequestException(string message) : base("Bad Request", message)
        {
        }
    }
}
