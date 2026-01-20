using Apex.Domain.Enums;
using Apex.Domain.Primitives;

namespace Apex.Domain.Entities;

/// <summary>
/// Represents a market index used for bond rate calculations.
/// Examples: PRE, CDI, IPCA, SELIC, etc.
/// </summary>
public sealed class MarketIndex : Entity<int>
{
    private MarketIndex(
        int id,
        string name,
        string description,
        MarketIndexType marketIndexType,
        string? virtualIndexName,
        string? cetipIndexName)
        : base(id)
    {
        Name = name;
        Description = description;
        MarketIndexType = marketIndexType;
        VirtualIndexName = virtualIndexName;
        CetipIndexName = cetipIndexName;
    }

    /// <summary>
    /// Gets the index name (e.g., "CDI", "IPCA", "SELIC").
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the detailed description of the index.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the type of market index.
    /// </summary>
    public MarketIndexType MarketIndexType { get; private set; }

    /// <summary>
    /// Gets the virtual index name used in the system.
    /// </summary>
    public string? VirtualIndexName { get; private set; }

    /// <summary>
    /// Gets the CETIP index name for integration.
    /// </summary>
    public string? CetipIndexName { get; private set; }

    /// <summary>
    /// Gets whether this is a pre-fixed index.
    /// </summary>
    public bool IsPreFixed => MarketIndexType == MarketIndexType.Pre;

    /// <summary>
    /// Gets whether this is a post-fixed index (CDI, SELIC, etc.).
    /// </summary>
    public bool IsPostFixed => MarketIndexType != MarketIndexType.Pre && MarketIndexType != MarketIndexType.NoIndex;

    /// <summary>
    /// Gets whether this is an inflation-linked index (IPCA, IGP-M).
    /// </summary>
    public bool IsInflationLinked => MarketIndexType == MarketIndexType.Ipca || MarketIndexType == MarketIndexType.IgpM;

    /// <summary>
    /// Creates a new MarketIndex instance (for new indices not yet persisted).
    /// </summary>
    /// <param name="name">The index name.</param>
    /// <param name="description">The description.</param>
    /// <param name="marketIndexType">The index type.</param>
    /// <param name="virtualIndexName">Optional virtual index name.</param>
    /// <param name="cetipIndexName">Optional CETIP index name.</param>
    /// <returns>A new MarketIndex instance.</returns>
    public static MarketIndex Create(
        string name,
        string description,
        MarketIndexType marketIndexType,
        string? virtualIndexName = null,
        string? cetipIndexName = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Market index name cannot be null or empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Market index description cannot be null or empty.", nameof(description));
        }

        return new MarketIndex(
            id: 0, // Will be set by database
            name: name.Trim(),
            description: description.Trim(),
            marketIndexType: marketIndexType,
            virtualIndexName: virtualIndexName?.Trim(),
            cetipIndexName: cetipIndexName?.Trim());
    }

    /// <summary>
    /// Reconstitutes a MarketIndex from persistence (used by repository).
    /// </summary>
    public static MarketIndex Reconstitute(
        int id,
        string name,
        string description,
        MarketIndexType marketIndexType,
        string? virtualIndexName,
        string? cetipIndexName)
    {
        return new MarketIndex(
            id: id,
            name: name,
            description: description,
            marketIndexType: marketIndexType,
            virtualIndexName: virtualIndexName,
            cetipIndexName: cetipIndexName);
    }

    /// <summary>
    /// Updates the market index information.
    /// </summary>
    public void Update(
        string name,
        string description,
        string? virtualIndexName = null,
        string? cetipIndexName = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Market index name cannot be null or empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Market index description cannot be null or empty.", nameof(description));
        }

        Name = name.Trim();
        Description = description.Trim();
        VirtualIndexName = virtualIndexName?.Trim();
        CetipIndexName = cetipIndexName?.Trim();
    }
}
