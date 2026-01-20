using Apex.Domain.Enums;
using Apex.Domain.Exceptions;
using Apex.Domain.Primitives;
using Apex.Domain.ValueObjects;

namespace Apex.Domain.Entities;

/// <summary>
/// Represents a fixed-income bond instrument.
/// This is an aggregate root that encapsulates the bond's core properties and business rules.
/// </summary>
public sealed class Bond : Entity<int>, IAuditable
{
    private Bond(
        int id,
        BondSymbol symbol,
        Isin isin,
        DateTime issuanceAt,
        DateTime expirationAt,
        int? bondDetailId,
        bool isCetipVerified,
        Guid apiId,
        DateTime createdAt)
        : base(id)
    {
        Symbol = symbol;
        Isin = isin;
        IssuanceAt = issuanceAt;
        ExpirationAt = expirationAt;
        BondDetailId = bondDetailId;
        IsCetipVerified = isCetipVerified;
        ApiId = apiId;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Gets the bond trading symbol.
    /// </summary>
    public BondSymbol Symbol { get; private set; }

    /// <summary>
    /// Gets the ISIN (International Securities Identification Number).
    /// </summary>
    public Isin Isin { get; private set; }

    /// <summary>
    /// Gets the bond's issuance date.
    /// </summary>
    public DateTime IssuanceAt { get; private set; }

    /// <summary>
    /// Gets the bond's expiration/maturity date.
    /// </summary>
    public DateTime ExpirationAt { get; private set; }

    /// <summary>
    /// Gets the foreign key to BondDetail table (nullable).
    /// </summary>
    public int? BondDetailId { get; private set; }

    /// <summary>
    /// Gets whether the bond has been verified by CETIP.
    /// </summary>
    public bool IsCetipVerified { get; private set; }

    /// <summary>
    /// Gets the API identifier (external system reference).
    /// </summary>
    public Guid ApiId { get; private set; }

    /// <summary>
    /// Gets the date and time when the bond was created in the system.
    /// </summary>
    public DateTime CreatedAt { get; private init; }

    /// <summary>
    /// Gets the date and time when the bond was last updated.
    /// </summary>
    public DateTime? LastUpdatedAt { get; private set; }

    /// <summary>
    /// Gets whether the bond has expired.
    /// </summary>
    public bool HasExpired => ExpirationAt.Date < DateTime.UtcNow.Date;

    /// <summary>
    /// Gets whether the bond is currently active (not expired and issuance date has passed).
    /// </summary>
    public bool IsActive => !HasExpired && IssuanceAt.Date <= DateTime.UtcNow.Date;

    /// <summary>
    /// Gets the remaining days until the bond expires.
    /// </summary>
    public int RemainingDays
    {
        get
        {
            var remainingDays = (ExpirationAt.Date - DateTime.UtcNow.Date).Days;
            return Math.Max(0, remainingDays);
        }
    }

    /// <summary>
    /// Gets the duration in calendar days between issuance and expiration.
    /// </summary>
    public int DurationInCalendarDays => (ExpirationAt.Date - IssuanceAt.Date).Days;

    /// <summary>
    /// Gets the duration in years (using 360-day year convention).
    /// </summary>
    public decimal DurationInYears => DurationInCalendarDays / 360m;

    /// <summary>
    /// Gets whether the bond has a linked BondDetail.
    /// </summary>
    public bool HasBondDetail => BondDetailId.HasValue;

    /// <summary>
    /// Gets whether the bond exists in the database (Id > 0).
    /// </summary>
    public bool ExistsInDatabase => Id > 0;

    /// <summary>
    /// Creates a new Bond instance (for new bonds not yet persisted).
    /// The ID will be 0 until persisted to the database.
    /// </summary>
    /// <param name="symbol">The bond trading symbol.</param>
    /// <param name="isin">The ISIN code.</param>
    /// <param name="issuanceAt">The bond issuance date.</param>
    /// <param name="expirationAt">The bond expiration date.</param>
    /// <param name="bondDetailId">Optional reference to BondDetail.</param>
    /// <param name="isCetipVerified">Whether CETIP verification is completed (default: false).</param>
    /// <param name="apiId">Optional API ID (will be generated if not provided).</param>
    /// <returns>A new Bond instance.</returns>
    /// <exception cref="InvalidBondException">Thrown when bond data is invalid.</exception>
    public static Bond Create(
        string symbol,
        string isin,
        DateTime issuanceAt,
        DateTime expirationAt,
        int? bondDetailId = null,
        bool isCetipVerified = false,
        Guid? apiId = null)
    {
        // Validate and create value objects
        var bondSymbol = BondSymbol.Create(symbol);
        var bondIsin = Isin.Create(isin);

        // Validate business rules
        ValidateDates(issuanceAt, expirationAt, symbol);

        var bond = new Bond(
            id: 0, // Will be set by database on insert
            symbol: bondSymbol,
            isin: bondIsin,
            issuanceAt: issuanceAt,
            expirationAt: expirationAt,
            bondDetailId: bondDetailId,
            isCetipVerified: isCetipVerified,
            apiId: apiId ?? Guid.NewGuid(),
            createdAt: DateTime.UtcNow);

        return bond;
    }

    /// <summary>
    /// Reconstitutes a Bond from persistence (used by repository).
    /// </summary>
    public static Bond Reconstitute(
        int id,
        string symbol,
        string isin,
        DateTime issuanceAt,
        DateTime expirationAt,
        int? bondDetailId,
        bool isCetipVerified,
        Guid apiId,
        DateTime createdAt,
        DateTime? lastUpdatedAt)
    {
        var bondSymbol = BondSymbol.Create(symbol);
        var bondIsin = Isin.Create(isin);

        var bond = new Bond(
            id: id,
            symbol: bondSymbol,
            isin: bondIsin,
            issuanceAt: issuanceAt,
            expirationAt: expirationAt,
            bondDetailId: bondDetailId,
            isCetipVerified: isCetipVerified,
            apiId: apiId,
            createdAt: createdAt)
        {
            LastUpdatedAt = lastUpdatedAt
        };

        return bond;
    }

    /// <summary>
    /// Updates the bond's CETIP verification status.
    /// </summary>
    /// <param name="isVerified">The new verification status.</param>
    public void UpdateCetipVerification(bool isVerified)
    {
        IsCetipVerified = isVerified;
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the API identifier.
    /// </summary>
    /// <param name="apiId">The new API ID.</param>
    public void UpdateApiId(Guid apiId)
    {
        if (apiId == Guid.Empty)
        {
            throw new InvalidBondException("API ID cannot be empty.");
        }

        ApiId = apiId;
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Links the bond to a BondDetail record.
    /// </summary>
    /// <param name="bondDetailId">The BondDetail ID.</param>
    /// <exception cref="InvalidBondException">Thrown when bondDetailId is invalid.</exception>
    public void LinkToBondDetail(int bondDetailId)
    {
        if (bondDetailId <= 0)
        {
            throw new InvalidBondException($"Invalid BondDetailId: {bondDetailId}. Must be greater than zero.");
        }

        BondDetailId = bondDetailId;
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Unlinks the bond from its BondDetail.
    /// </summary>
    public void UnlinkFromBondDetail()
    {
        BondDetailId = null;
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Extends the bond's expiration date.
    /// </summary>
    /// <param name="newExpirationAt">The new expiration date (must be after current expiration).</param>
    /// <exception cref="InvalidBondException">Thrown when new expiration date is invalid.</exception>
    public void ExtendExpiration(DateTime newExpirationAt)
    {
        if (newExpirationAt <= ExpirationAt)
        {
            throw new InvalidBondException($"New expiration date must be after current expiration date ({ExpirationAt:yyyy-MM-dd}).");
        }

        if (newExpirationAt <= IssuanceAt)
        {
            throw new InvalidBondException($"Expiration date must be after issuance date ({IssuanceAt:yyyy-MM-dd}).");
        }

        ExpirationAt = newExpirationAt;
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the bond symbol.
    /// </summary>
    /// <param name="newSymbol">The new bond symbol.</param>
    public void UpdateSymbol(string newSymbol)
    {
        Symbol = BondSymbol.Create(newSymbol);
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the bond ISIN.
    /// </summary>
    /// <param name="newIsin">The new ISIN code.</param>
    public void UpdateIsin(string newIsin)
    {
        Isin = Isin.Create(newIsin);
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Ensures the bond is active before performing operations.
    /// </summary>
    /// <exception cref="BondNotActiveException">Thrown when bond is not active.</exception>
    public void EnsureIsActive()
    {
        if (!IsActive)
        {
            throw new BondNotActiveException(Symbol);
        }
    }

    /// <summary>
    /// Ensures the bond has not expired.
    /// </summary>
    /// <exception cref="BondExpiredException">Thrown when bond has expired.</exception>
    public void EnsureNotExpired()
    {
        if (HasExpired)
        {
            throw new BondExpiredException(Symbol, ExpirationAt);
        }
    }

    /// <summary>
    /// Ensures CETIP verification is completed.
    /// </summary>
    /// <exception cref="CetipVerificationException">Thrown when verification is not completed.</exception>
    public void EnsureCetipVerified()
    {
        if (!IsCetipVerified)
        {
            throw new CetipVerificationException(Symbol, "Bond requires CETIP verification before trading.");
        }
    }

    private static void ValidateDates(DateTime issuanceAt, DateTime expirationAt, string symbol)
    {
        if (expirationAt <= issuanceAt)
        {
            throw new InvalidBondException($"Expiration date must be after issuance date for bond '{symbol}'.");
        }

        // Allow creation of bonds with expiration in the past for historical data
        // But issue a warning through the domain
        if (expirationAt.Date < DateTime.UtcNow.Date)
        {
            // This is allowed for data migration and historical records
            // The HasExpired property will handle the business logic
        }
    }
}
