using Apex.Domain.Primitives;

namespace Apex.Domain.Exceptions;

/// <summary>
/// Exception thrown when an invalid operation is attempted on a customer.
/// </summary>
public sealed class InvalidCustomerOperationException : DomainException
{
    public InvalidCustomerOperationException(string message)
        : base(message)
    {
    }

    public InvalidCustomerOperationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a customer is not found.
/// </summary>
public sealed class CustomerNotFoundException : DomainException
{
    public CustomerNotFoundException(string identifier)
        : base($"Customer not found with identifier: {identifier}")
    {
        Identifier = identifier;
    }

    public string Identifier { get; }
}

/// <summary>
/// Exception thrown when attempting to create a duplicate customer.
/// </summary>
public sealed class DuplicateCustomerException : DomainException
{
    public DuplicateCustomerException(string document, string sinacorId, string company)
        : base($"Customer with document '{document}', Sinacor ID '{sinacorId}' and company '{company}' already exists.")
    {
        Document = document;
        SinacorId = sinacorId;
        Company = company;
    }

    public string Document { get; }
    public string SinacorId { get; }
    public string Company { get; }
}
