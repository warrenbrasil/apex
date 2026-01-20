using Apex.Domain.Primitives;

namespace Apex.Domain.ValueObjects;

/// <summary>
/// Value object representing a date range with start and end dates.
/// Used for bond issuance and expiration dates.
/// </summary>
public sealed class DateRange : ValueObject
{
    private DateRange(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>
    /// Gets the start date of the range.
    /// </summary>
    public DateTime StartDate { get; }

    /// <summary>
    /// Gets the end date of the range.
    /// </summary>
    public DateTime EndDate { get; }

    /// <summary>
    /// Gets the duration in days between start and end dates.
    /// </summary>
    public int DurationInDays => (EndDate.Date - StartDate.Date).Days;

    /// <summary>
    /// Gets the duration in years (using 360-day year convention).
    /// </summary>
    public decimal DurationInYears => DurationInDays / 360m;

    /// <summary>
    /// Gets whether the date range has expired (end date is in the past).
    /// </summary>
    public bool HasExpired => EndDate.Date < DateTime.UtcNow.Date;

    /// <summary>
    /// Gets whether the date range is currently active.
    /// </summary>
    public bool IsActive => !HasExpired && StartDate.Date <= DateTime.UtcNow.Date;

    /// <summary>
    /// Creates a new DateRange instance.
    /// </summary>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <returns>A new DateRange instance.</returns>
    public static DateRange Create(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));
        }

        return new DateRange(startDate, endDate);
    }

    /// <summary>
    /// Calculates the remaining days until expiration.
    /// </summary>
    /// <returns>Number of days remaining, or 0 if expired.</returns>
    public int GetRemainingDays()
    {
        var remainingDays = (EndDate.Date - DateTime.UtcNow.Date).Days;
        return Math.Max(0, remainingDays);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return StartDate;
        yield return EndDate;
    }

    public override string ToString() => $"{StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}";
}
