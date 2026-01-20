using Apex.Domain.Primitives;

namespace Apex.Domain.ValueObjects;

/// <summary>
/// Value object representing a monetary amount in BRL (Brazilian Real).
/// </summary>
public sealed class Money : ValueObject
{
    private Money(decimal amount)
    {
        Amount = amount;
    }

    /// <summary>
    /// Gets the monetary amount.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Gets whether the amount is zero.
    /// </summary>
    public bool IsZero => Amount == 0;

    /// <summary>
    /// Gets whether the amount is positive.
    /// </summary>
    public bool IsPositive => Amount > 0;

    /// <summary>
    /// Gets whether the amount is negative.
    /// </summary>
    public bool IsNegative => Amount < 0;

    /// <summary>
    /// Creates a new Money instance.
    /// </summary>
    /// <param name="amount">The monetary amount.</param>
    /// <returns>A new Money instance.</returns>
    public static Money Create(decimal amount)
    {
        return new Money(amount);
    }

    /// <summary>
    /// Creates a zero money instance.
    /// </summary>
    public static Money Zero => new(0);

    /// <summary>
    /// Adds two money amounts.
    /// </summary>
    public static Money operator +(Money left, Money right)
    {
        return new Money(left.Amount + right.Amount);
    }

    /// <summary>
    /// Subtracts two money amounts.
    /// </summary>
    public static Money operator -(Money left, Money right)
    {
        return new Money(left.Amount - right.Amount);
    }

    /// <summary>
    /// Multiplies money by a factor.
    /// </summary>
    public static Money operator *(Money money, decimal factor)
    {
        return new Money(money.Amount * factor);
    }

    /// <summary>
    /// Divides money by a divisor.
    /// </summary>
    public static Money operator /(Money money, decimal divisor)
    {
        if (divisor == 0)
        {
            throw new DivideByZeroException("Cannot divide money by zero.");
        }

        return new Money(money.Amount / divisor);
    }

    /// <summary>
    /// Formats the money amount for display.
    /// </summary>
    /// <returns>Formatted string with BRL currency symbol.</returns>
    public string FormatForDisplay()
    {
        return $"R$ {Amount:N2}";
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
    }

    public override string ToString() => FormatForDisplay();

    public static implicit operator decimal(Money money) => money.Amount;
}
