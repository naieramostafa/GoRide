namespace RideSharing.Core.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class RideStateException : DomainException
{
    public RideStateException(string message) : base(message) { }
}

public class PaymentFailedException : DomainException
{
    public PaymentFailedException(string message) : base(message) { }
}

public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, Guid id)
        : base($"{entityName} with id {id} was not found") { }
}
