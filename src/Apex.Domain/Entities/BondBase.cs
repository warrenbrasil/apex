using Apex.Domain.Enums;
using Apex.Domain.Primitives;

namespace Apex.Domain.Entities;

/// <summary>
/// Represents the base type/category of a bond (CDB, CRI, CRA, Debenture, etc.).
/// </summary>
public sealed class BondBase : Entity<int>
{
    private BondBase(
        int id,
        string baseSymbol,
        string description,
        short typeCore,
        CustodyChamberType custodyChamber,
        bool guaranteedByFgc,
        bool hasIncomeTax)
        : base(id)
    {
        BaseSymbol = baseSymbol;
        Description = description;
        TypeCore = typeCore;
        CustodyChamber = custodyChamber;
        GuaranteedByFgc = guaranteedByFgc;
        HasIncomeTax = hasIncomeTax;
    }

    /// <summary>
    /// Gets the base symbol (e.g., "CDB", "CRI", "CRA", "DEB").
    /// </summary>
    public string BaseSymbol { get; private set; }

    /// <summary>
    /// Gets the description of the bond type.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the type identifier for core system integration.
    /// </summary>
    public short TypeCore { get; private set; }

    /// <summary>
    /// Gets the custody chamber where bonds of this type are held.
    /// </summary>
    public CustodyChamberType CustodyChamber { get; private set; }

    /// <summary>
    /// Gets whether bonds of this type are guaranteed by FGC (Fundo Garantidor de Créditos).
    /// FGC provides coverage up to R$ 250,000 per CPF/CNPJ per institution.
    /// </summary>
    public bool GuaranteedByFgc { get; private set; }

    /// <summary>
    /// Gets whether bonds of this type are subject to income tax.
    /// </summary>
    public bool HasIncomeTax { get; private set; }

    /// <summary>
    /// Gets whether this bond type is in CETIP custody.
    /// </summary>
    public bool IsCetipCustody => CustodyChamber == CustodyChamberType.Cetip;

    /// <summary>
    /// Gets whether this bond type is in SELIC custody.
    /// </summary>
    public bool IsSelicCustody => CustodyChamber == CustodyChamberType.Selic;

    /// <summary>
    /// Creates a new BondBase instance (for new bond types not yet persisted).
    /// </summary>
    /// <param name="baseSymbol">The base symbol (e.g., "CDB", "CRI").</param>
    /// <param name="description">The description.</param>
    /// <param name="typeCore">The core type identifier.</param>
    /// <param name="custodyChamber">The custody chamber type.</param>
    /// <param name="guaranteedByFgc">Whether guaranteed by FGC.</param>
    /// <param name="hasIncomeTax">Whether subject to income tax.</param>
    /// <returns>A new BondBase instance.</returns>
    public static BondBase Create(
        string baseSymbol,
        string description,
        short typeCore,
        CustodyChamberType custodyChamber,
        bool guaranteedByFgc = false,
        bool hasIncomeTax = true)
    {
        if (string.IsNullOrWhiteSpace(baseSymbol))
        {
            throw new ArgumentException("Base symbol cannot be null or empty.", nameof(baseSymbol));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description cannot be null or empty.", nameof(description));
        }

        if (baseSymbol.Length > 10)
        {
            throw new ArgumentException("Base symbol cannot exceed 10 characters.", nameof(baseSymbol));
        }

        return new BondBase(
            id: 0, // Will be set by database
            baseSymbol: baseSymbol.Trim().ToUpperInvariant(),
            description: description.Trim(),
            typeCore: typeCore,
            custodyChamber: custodyChamber,
            guaranteedByFgc: guaranteedByFgc,
            hasIncomeTax: hasIncomeTax);
    }

    /// <summary>
    /// Reconstitutes a BondBase from persistence (used by repository).
    /// </summary>
    public static BondBase Reconstitute(
        int id,
        string baseSymbol,
        string description,
        short typeCore,
        CustodyChamberType custodyChamber,
        bool guaranteedByFgc,
        bool hasIncomeTax)
    {
        return new BondBase(
            id: id,
            baseSymbol: baseSymbol,
            description: description,
            typeCore: typeCore,
            custodyChamber: custodyChamber,
            guaranteedByFgc: guaranteedByFgc,
            hasIncomeTax: hasIncomeTax);
    }

    /// <summary>
    /// Updates the bond base information.
    /// </summary>
    public void Update(
        string description,
        bool guaranteedByFgc,
        bool hasIncomeTax)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description cannot be null or empty.", nameof(description));
        }

        Description = description.Trim();
        GuaranteedByFgc = guaranteedByFgc;
        HasIncomeTax = hasIncomeTax;
    }
}
