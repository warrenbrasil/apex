using Apex.Domain.Enums;
using Apex.Domain.Primitives;
using Apex.Domain.ValueObjects;
using System.Net.Mail;

namespace Apex.Domain.Entities;

/// <summary>
/// Represents an entity that issues bonds (emitter/issuer).
/// Can be a financial institution, company, or government entity.
/// </summary>
public sealed class BondEmitter : Entity<int>
{
    private BondEmitter(
        int id,
        string issuer,
        string name,
        string fullName,
        BusinessDocument businessDocument,
        string? email,
        CreditRating creditRating,
        string? externalId,
        EmitterType issuerType)
        : base(id)
    {
        Issuer = issuer;
        Name = name;
        FullName = fullName;
        BusinessDocument = businessDocument;
        Email = email;
        CreditRating = creditRating;
        ExternalId = externalId;
        IssuerType = issuerType;
    }

    /// <summary>
    /// Gets the issuer code/identifier.
    /// </summary>
    public string Issuer { get; private set; }

    /// <summary>
    /// Gets the short name of the emitter.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the full legal name of the emitter.
    /// </summary>
    public string FullName { get; private set; }

    /// <summary>
    /// Gets the business document (CNPJ or CPF).
    /// </summary>
    public BusinessDocument BusinessDocument { get; private set; }

    /// <summary>
    /// Gets the email address for contact.
    /// </summary>
    public string? Email { get; private set; }

    /// <summary>
    /// Gets the credit rating of the emitter.
    /// </summary>
    public CreditRating CreditRating { get; private set; }

    /// <summary>
    /// Gets the external system identifier.
    /// </summary>
    public string? ExternalId { get; private set; }

    /// <summary>
    /// Gets the type of issuer (Financial Institution, Company, Government, etc.).
    /// </summary>
    public EmitterType IssuerType { get; private set; }

    /// <summary>
    /// Gets whether this is a financial institution.
    /// </summary>
    public bool IsFinancialInstitution => IssuerType == EmitterType.FinancialInstitution;

    /// <summary>
    /// Gets whether this is a private company.
    /// </summary>
    public bool IsCompany => IssuerType == EmitterType.Company;

    /// <summary>
    /// Gets whether this is a government entity (Union, State, or City).
    /// </summary>
    public bool IsGovernment => IssuerType is EmitterType.Union or EmitterType.State or EmitterType.City;

    /// <summary>
    /// Gets whether the credit rating is high quality.
    /// </summary>
    public bool IsHighCreditRating => CreditRating == CreditRating.High;

    /// <summary>
    /// Gets whether the credit rating is investment grade (medium or high).
    /// </summary>
    public bool IsInvestmentGrade => CreditRating is CreditRating.Medium or CreditRating.High;

    /// <summary>
    /// Creates a new BondEmitter instance (for new emitters not yet persisted).
    /// </summary>
    /// <param name="issuer">The issuer code.</param>
    /// <param name="name">The short name.</param>
    /// <param name="fullName">The full legal name.</param>
    /// <param name="businessDocument">The CNPJ or CPF.</param>
    /// <param name="email">Optional email address.</param>
    /// <param name="creditRating">The credit rating.</param>
    /// <param name="externalId">Optional external system ID.</param>
    /// <param name="issuerType">The type of issuer.</param>
    /// <returns>A new BondEmitter instance.</returns>
    public static BondEmitter Create(
        string issuer,
        string name,
        string fullName,
        string businessDocument,
        string? email,
        CreditRating creditRating,
        string? externalId,
        EmitterType issuerType)
    {
        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new ArgumentException("Issuer code cannot be null or empty.", nameof(issuer));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name cannot be null or empty.", nameof(fullName));
        }

        var document = BusinessDocument.Create(businessDocument);

        // Validate email if provided
        if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
        {
            throw new ArgumentException("Invalid email format.", nameof(email));
        }

        return new BondEmitter(
            id: 0, // Will be set by database
            issuer: issuer.Trim(),
            name: name.Trim(),
            fullName: fullName.Trim(),
            businessDocument: document,
            email: email?.Trim(),
            creditRating: creditRating,
            externalId: externalId?.Trim(),
            issuerType: issuerType);
    }

    /// <summary>
    /// Reconstitutes a BondEmitter from persistence (used by repository).
    /// </summary>
    public static BondEmitter Reconstitute(
        int id,
        string issuer,
        string name,
        string fullName,
        string businessDocument,
        string? email,
        CreditRating creditRating,
        string? externalId,
        EmitterType issuerType)
    {
        var document = BusinessDocument.Create(businessDocument);

        return new BondEmitter(
            id: id,
            issuer: issuer,
            name: name,
            fullName: fullName,
            businessDocument: document,
            email: email,
            creditRating: creditRating,
            externalId: externalId,
            issuerType: issuerType);
    }

    /// <summary>
    /// Updates the emitter information.
    /// </summary>
    public void Update(
        string name,
        string fullName,
        string? email,
        CreditRating creditRating)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name cannot be null or empty.", nameof(fullName));
        }

        if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
        {
            throw new ArgumentException("Invalid email format.", nameof(email));
        }

        Name = name.Trim();
        FullName = fullName.Trim();
        Email = email?.Trim();
        CreditRating = creditRating;
    }

    /// <summary>
    /// Updates the credit rating.
    /// </summary>
    public void UpdateCreditRating(CreditRating newRating)
    {
        CreditRating = newRating;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
