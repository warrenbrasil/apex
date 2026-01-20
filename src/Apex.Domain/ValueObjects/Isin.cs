using Apex.Domain.Primitives;

namespace Apex.Domain.ValueObjects;

/// <summary>
/// Value object representing an ISIN (International Securities Identification Number).
/// ISIN is a 12-character alphanumeric code that uniquely identifies a financial security.
/// Format: 2 letters (country code) + 9 alphanumeric + 1 check digit.
/// Example: BRXXXXAAAXXX
/// </summary>
public sealed class Isin : ValueObject
{
    private const int IsinLength = 12;

    private Isin(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Isin Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("ISIN cannot be null or empty.", nameof(value));
        }

        var trimmedValue = value.Trim().ToUpperInvariant();

        if (trimmedValue.Length != IsinLength)
        {
            throw new ArgumentException($"ISIN must be exactly {IsinLength} characters long.", nameof(value));
        }

        if (!IsValidFormat(trimmedValue))
        {
            throw new ArgumentException("ISIN format is invalid. Must start with 2 letters followed by 10 alphanumeric characters.", nameof(value));
        }

        return new Isin(trimmedValue);
    }

    private static bool IsValidFormat(string value)
    {
        // First two characters must be letters (country code)
        if (!char.IsLetter(value[0]) || !char.IsLetter(value[1]))
        {
            return false;
        }

        // Remaining characters must be alphanumeric
        for (int i = 2; i < value.Length; i++)
        {
            if (!char.IsLetterOrDigit(value[i]))
            {
                return false;
            }
        }

        return true;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Isin isin) => isin.Value;
}
