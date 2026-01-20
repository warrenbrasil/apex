using Apex.Domain.Primitives;

namespace Apex.Domain.ValueObjects;

/// <summary>
/// Value object representing an interest rate percentage.
/// Rates are stored as percentages (e.g., 10.5 for 10.5% per year).
/// </summary>
public sealed class Rate : ValueObject
{
    private Rate(decimal value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the rate value as a percentage.
    /// </summary>
    public decimal Value { get; }

    /// <summary>
    /// Gets whether the rate is zero.
    /// </summary>
    public bool IsZero => Value == 0;

    /// <summary>
    /// Creates a new Rate instance.
    /// </summary>
    /// <param name="value">The rate percentage value.</param>
    /// <returns>A new Rate instance.</returns>
    public static Rate Create(decimal value)
    {
        if (value < 0)
        {
            throw new ArgumentException("Rate cannot be negative.", nameof(value));
        }

        if (value > 1000)
        {
            throw new ArgumentException("Rate cannot exceed 1000%.", nameof(value));
        }

        return new Rate(value);
    }

    /// <summary>
    /// Creates a zero rate.
    /// </summary>
    public static Rate Zero => new(0);

    /// <summary>
    /// Formats the rate for display.
    /// </summary>
    /// <param name="decimalPlaces">Number of decimal places (default: 2).</param>
    /// <returns>Formatted rate string with percentage symbol.</returns>
    public string FormatForDisplay(int decimalPlaces = 2)
    {
        return $"{Value.ToString($"F{decimalPlaces}")}%";
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => FormatForDisplay();

    public static implicit operator decimal(Rate rate) => rate.Value;
}
