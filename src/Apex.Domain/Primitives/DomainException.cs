namespace Apex.Domain.Primitives;

/// <summary>
/// Base exception for all domain-level exceptions.
/// Domain exceptions represent business rule violations.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message)
        : base(message)
    {
    }

    protected DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
