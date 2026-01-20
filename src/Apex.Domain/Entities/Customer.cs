using Apex.Domain.Enums;
using Apex.Domain.Exceptions;
using Apex.Domain.Primitives;
using Apex.Domain.ValueObjects;

namespace Apex.Domain.Entities;

/// <summary>
/// Represents a customer in the system.
/// This is an aggregate root that manages customer identification and external system registrations.
/// </summary>
public sealed class Customer : Entity<int>, IAuditable
{
    private readonly List<CustomerExternalSystemRegister> _externalRegisters = new();

    private Customer(
        int id,
        string apiId,
        BusinessDocument document,
        string? sinacorId,
        Company company,
        string? legacyExternalId,
        DateTime createdAt)
        : base(id)
    {
        ApiId = apiId;
        Document = document;
        SinacorId = sinacorId;
        Company = company;
        LegacyExternalId = legacyExternalId;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Gets the API identifier (external system reference).
    /// </summary>
    public string ApiId { get; private set; }

    /// <summary>
    /// Gets the customer's business document (CPF or CNPJ).
    /// </summary>
    public BusinessDocument Document { get; private set; }

    /// <summary>
    /// Gets the Sinacor identifier (Brazilian stock exchange system).
    /// </summary>
    public string? SinacorId { get; private set; }

    /// <summary>
    /// Gets the company that manages this customer (Warren or Rena).
    /// </summary>
    public Company Company { get; private set; }

    /// <summary>
    /// Gets the legacy external identifier (for migration purposes).
    /// </summary>
    public string? LegacyExternalId { get; private set; }

    /// <summary>
    /// Gets the date and time when the customer was created.
    /// </summary>
    public DateTime CreatedAt { get; private init; }

    /// <summary>
    /// Gets the date and time when the customer was last updated.
    /// </summary>
    public DateTime? LastUpdatedAt { get; private set; }

    /// <summary>
    /// Gets the collection of external system registrations (CETIP, SELIC).
    /// </summary>
    public IReadOnlyList<CustomerExternalSystemRegister> ExternalRegisters => _externalRegisters.AsReadOnly();

    /// <summary>
    /// Gets whether the customer has a Sinacor ID.
    /// </summary>
    public bool HasSinacorId => !string.IsNullOrWhiteSpace(SinacorId);

    /// <summary>
    /// Gets whether the customer has a legacy external ID.
    /// </summary>
    public bool HasLegacyExternalId => !string.IsNullOrWhiteSpace(LegacyExternalId);

    /// <summary>
    /// Gets whether the customer is managed by Warren.
    /// </summary>
    public bool IsWarrenCustomer => Company == Company.Warren;

    /// <summary>
    /// Gets whether the customer is managed by Rena.
    /// </summary>
    public bool IsRenaCustomer => Company == Company.Rena;

    /// <summary>
    /// Gets whether the customer is an individual (CPF).
    /// </summary>
    public bool IsIndividual => Document.IsCpf;

    /// <summary>
    /// Gets whether the customer is a legal entity (CNPJ).
    /// </summary>
    public bool IsLegalEntity => Document.IsCnpj;

    /// <summary>
    /// Creates a new Customer instance (for new customers not yet persisted).
    /// Automatically initializes external system registers for CETIP and SELIC.
    /// </summary>
    /// <param name="apiId">The API identifier (max 32 chars).</param>
    /// <param name="document">The CPF or CNPJ (11 or 14 digits).</param>
    /// <param name="company">The company managing the customer.</param>
    /// <param name="sinacorId">Optional Sinacor identifier (max 9 chars).</param>
    /// <param name="legacyExternalId">Optional legacy external ID (max 9 chars).</param>
    /// <returns>A new Customer instance.</returns>
    public static Customer Create(
        string apiId,
        string document,
        Company company,
        string? sinacorId = null,
        string? legacyExternalId = null)
    {
        ValidateApiId(apiId);
        ValidateSinacorId(sinacorId);
        ValidateLegacyExternalId(legacyExternalId);

        var businessDocument = BusinessDocument.Create(document);

        var customer = new Customer(
            id: 0, // Will be set by database
            apiId: apiId.Trim(),
            document: businessDocument,
            sinacorId: sinacorId?.Trim(),
            company: company,
            legacyExternalId: legacyExternalId?.Trim(),
            createdAt: DateTime.UtcNow);

        // Initialize external system registers
        customer.InitializeExternalRegisters();

        return customer;
    }

    /// <summary>
    /// Reconstitutes a Customer from persistence (used by repository).
    /// </summary>
    public static Customer Reconstitute(
        int id,
        string apiId,
        string document,
        string? sinacorId,
        Company company,
        string? legacyExternalId,
        DateTime createdAt,
        DateTime? lastUpdatedAt,
        List<CustomerExternalSystemRegister>? externalRegisters = null)
    {
        var businessDocument = BusinessDocument.Create(document);

        var customer = new Customer(
            id: id,
            apiId: apiId,
            document: businessDocument,
            sinacorId: sinacorId,
            company: company,
            legacyExternalId: legacyExternalId,
            createdAt: createdAt)
        {
            LastUpdatedAt = lastUpdatedAt
        };

        // Add external registers if provided
        if (externalRegisters != null)
        {
            customer._externalRegisters.AddRange(externalRegisters);
        }

        return customer;
    }

    /// <summary>
    /// Updates the customer's API ID.
    /// </summary>
    public void UpdateApiId(string newApiId)
    {
        ValidateApiId(newApiId);
        ApiId = newApiId.Trim();
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the customer's Sinacor ID.
    /// </summary>
    public void UpdateSinacorId(string? newSinacorId)
    {
        ValidateSinacorId(newSinacorId);
        SinacorId = newSinacorId?.Trim();
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the customer's company.
    /// </summary>
    public void UpdateCompany(Company newCompany)
    {
        Company = newCompany;
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the customer's legacy external ID.
    /// </summary>
    public void UpdateLegacyExternalId(string? newLegacyExternalId)
    {
        ValidateLegacyExternalId(newLegacyExternalId);
        LegacyExternalId = newLegacyExternalId?.Trim();
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the customer's document.
    /// </summary>
    public void UpdateDocument(string newDocument)
    {
        Document = BusinessDocument.Create(newDocument);
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the external system register for a specific system type.
    /// </summary>
    public CustomerExternalSystemRegister? GetRegisterForSystem(CustomerExternalSystemType systemType)
    {
        return _externalRegisters.FirstOrDefault(r => r.SystemType == systemType);
    }

    /// <summary>
    /// Gets the CETIP registration status.
    /// </summary>
    public CustomerExternalSystemRegister? GetCetipRegister()
    {
        return GetRegisterForSystem(CustomerExternalSystemType.Cetip);
    }

    /// <summary>
    /// Gets the SELIC registration status.
    /// </summary>
    public CustomerExternalSystemRegister? GetSelicRegister()
    {
        return GetRegisterForSystem(CustomerExternalSystemType.Selic);
    }

    /// <summary>
    /// Checks if the customer is registered in a specific external system.
    /// </summary>
    public bool IsRegisteredIn(CustomerExternalSystemType systemType)
    {
        var register = GetRegisterForSystem(systemType);
        return register?.IsRegistered ?? false;
    }

    /// <summary>
    /// Checks if the customer is registered in CETIP.
    /// </summary>
    public bool IsRegisteredInCetip()
    {
        return IsRegisteredIn(CustomerExternalSystemType.Cetip);
    }

    /// <summary>
    /// Checks if the customer is registered in SELIC.
    /// </summary>
    public bool IsRegisteredInSelic()
    {
        return IsRegisteredIn(CustomerExternalSystemType.Selic);
    }

    /// <summary>
    /// Marks the customer as registered in a specific external system.
    /// </summary>
    public void MarkAsRegisteredIn(CustomerExternalSystemType systemType)
    {
        var register = GetRegisterForSystem(systemType);
        if (register == null)
        {
            throw new InvalidCustomerOperationException($"No register found for system type {systemType}.");
        }

        register.MarkAsRegistered();
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the customer as inactive in a specific external system.
    /// </summary>
    public void MarkAsInactiveIn(CustomerExternalSystemType systemType)
    {
        var register = GetRegisterForSystem(systemType);
        if (register == null)
        {
            throw new InvalidCustomerOperationException($"No register found for system type {systemType}.");
        }

        register.MarkAsInactive();
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes the external system registers (CETIP and SELIC) with NotRegistered status.
    /// Called automatically during customer creation.
    /// </summary>
    private void InitializeExternalRegisters()
    {
        // Note: CustomerId will be 0 until persisted, but will be set by repository
        _externalRegisters.Add(CustomerExternalSystemRegister.Create(
            CustomerExternalSystemType.Cetip,
            Id,
            CustomerExternalSystemStatus.NotRegistered));

        _externalRegisters.Add(CustomerExternalSystemRegister.Create(
            CustomerExternalSystemType.Selic,
            Id,
            CustomerExternalSystemStatus.NotRegistered));
    }

    private static void ValidateApiId(string apiId)
    {
        if (string.IsNullOrWhiteSpace(apiId))
        {
            throw new ArgumentException("API ID cannot be null or empty.", nameof(apiId));
        }

        if (apiId.Trim().Length > 32)
        {
            throw new ArgumentException("API ID cannot exceed 32 characters.", nameof(apiId));
        }
    }

    private static void ValidateSinacorId(string? sinacorId)
    {
        if (sinacorId != null && sinacorId.Trim().Length > 9)
        {
            throw new ArgumentException("Sinacor ID cannot exceed 9 characters.", nameof(sinacorId));
        }
    }

    private static void ValidateLegacyExternalId(string? legacyExternalId)
    {
        if (legacyExternalId != null && legacyExternalId.Trim().Length > 9)
        {
            throw new ArgumentException("Legacy External ID cannot exceed 9 characters.", nameof(legacyExternalId));
        }
    }
}
