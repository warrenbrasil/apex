namespace Apex.Domain.Primitives;

/// <summary>
/// Interface for entities that support audit tracking.
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// Gets the date and time when the entity was created.
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// Gets the date and time when the entity was last updated.
    /// </summary>
    DateTime? LastUpdatedAt { get; }
}
