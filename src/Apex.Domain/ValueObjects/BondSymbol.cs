using Apex.Domain.Primitives;

namespace Apex.Domain.ValueObjects;

/// <summary>
/// Value object representing a bond trading symbol.
/// The symbol uniquely identifies a bond in trading systems.
/// </summary>
public sealed class BondSymbol : ValueObject
{
    private const int MaxLength = 50;

    private BondSymbol(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static BondSymbol Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Bond symbol cannot be null or empty.", nameof(value));
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length > MaxLength)
        {
            throw new ArgumentException($"Bond symbol cannot exceed {MaxLength} characters.", nameof(value));
        }

        return new BondSymbol(trimmedValue);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(BondSymbol symbol) => symbol.Value;
}
