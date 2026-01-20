using Apex.Domain.Primitives;

namespace Apex.Domain.Exceptions;

/// <summary>
/// Exception thrown when attempting to create or modify a bond with invalid data.
/// </summary>
public sealed class InvalidBondException : DomainException
{
    public InvalidBondException(string message)
        : base(message)
    {
    }

    public InvalidBondException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a bond has already expired.
/// </summary>
public sealed class BondExpiredException : DomainException
{
    public BondExpiredException(string bondSymbol, DateTime expirationDate)
        : base($"Bond '{bondSymbol}' has expired on {expirationDate:yyyy-MM-dd}.")
    {
        BondSymbol = bondSymbol;
        ExpirationDate = expirationDate;
    }

    public string BondSymbol { get; }
    public DateTime ExpirationDate { get; }
}

/// <summary>
/// Exception thrown when attempting operations on an inactive bond.
/// </summary>
public sealed class BondNotActiveException : DomainException
{
    public BondNotActiveException(string bondSymbol)
        : base($"Bond '{bondSymbol}' is not currently active.")
    {
        BondSymbol = bondSymbol;
    }

    public string BondSymbol { get; }
}

/// <summary>
/// Exception thrown when CETIP verification fails or is required.
/// </summary>
public sealed class CetipVerificationException : DomainException
{
    public CetipVerificationException(string bondSymbol, string reason)
        : base($"CETIP verification failed for bond '{bondSymbol}': {reason}")
    {
        BondSymbol = bondSymbol;
        Reason = reason;
    }

    public string BondSymbol { get; }
    public string Reason { get; }
}
