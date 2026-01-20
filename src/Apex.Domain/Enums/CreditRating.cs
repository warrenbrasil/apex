namespace Apex.Domain.Enums;

/// <summary>
/// Represents the credit rating classification of a bond emitter.
/// </summary>
public enum CreditRating
{
    /// <summary>
    /// Low credit rating - BB+ and lower.
    /// Higher risk, potentially higher returns.
    /// </summary>
    Low = 10,

    /// <summary>
    /// Medium credit rating - Up to A+.
    /// Moderate risk profile.
    /// </summary>
    Medium = 20,

    /// <summary>
    /// High credit rating - Better than A+.
    /// Lower risk, typically lower returns.
    /// </summary>
    High = 30
}
