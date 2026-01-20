using Apex.Domain.Primitives;
using Apex.Domain.ValueObjects;

namespace Apex.Domain.Entities;

/// <summary>
/// Represents detailed information about a bond offering.
/// Contains rates, deadlines, and relationships to base, emitter, and index.
/// This is an aggregate root.
/// </summary>
public sealed class BondDetail : Entity<int>, IAuditable
{
    private BondDetail(
        int id,
        string? fantasyName,
        int deadlineCalendarDays,
        Money initialUnitValue,
        Rate benchmarkPercentualRate,
        Rate fixedPercentualRate,
        bool isAvailable,
        bool isExemptDebenture,
        int daysToGracePeriod,
        int marketIndexId,
        int bondBaseId,
        int bondEmitterId,
        DateTime createdAt)
        : base(id)
    {
        FantasyName = fantasyName;
        DeadlineCalendarDays = deadlineCalendarDays;
        InitialUnitValue = initialUnitValue;
        BenchmarkPercentualRate = benchmarkPercentualRate;
        FixedPercentualRate = fixedPercentualRate;
        IsAvailable = isAvailable;
        IsExemptDebenture = isExemptDebenture;
        DaysToGracePeriod = daysToGracePeriod;
        MarketIndexId = marketIndexId;
        BondBaseId = bondBaseId;
        BondEmitterId = bondEmitterId;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Gets the fantasy/display name of the bond.
    /// </summary>
    public string? FantasyName { get; private set; }

    /// <summary>
    /// Gets the deadline in calendar days until maturity.
    /// </summary>
    public int DeadlineCalendarDays { get; private set; }

    /// <summary>
    /// Gets the deadline in years (using 360-day year convention).
    /// </summary>
    public decimal DeadlineCalendarYears => Math.Round(DeadlineCalendarDays / 360m, 1);

    /// <summary>
    /// Gets the initial unit value/price of the bond.
    /// </summary>
    public Money InitialUnitValue { get; private set; }

    /// <summary>
    /// Gets the benchmark rate (for post-fixed bonds like CDI+, IPCA+).
    /// Example: CDI + 2.5% means BenchmarkPercentualRate = 2.5
    /// </summary>
    public Rate BenchmarkPercentualRate { get; private set; }

    /// <summary>
    /// Gets the fixed rate (for pre-fixed bonds or hybrid bonds).
    /// Example: Pre-fixed 10.5% means FixedPercentualRate = 10.5
    /// </summary>
    public Rate FixedPercentualRate { get; private set; }

    /// <summary>
    /// Gets whether this bond detail is available for trading.
    /// </summary>
    public bool IsAvailable { get; private set; }

    /// <summary>
    /// Gets whether this is an exempt debenture (tax-free).
    /// </summary>
    public bool IsExemptDebenture { get; private set; }

    /// <summary>
    /// Gets the grace period in days (time until liquidity is available).
    /// 0 = daily liquidity, same as DeadlineCalendarDays = maturity only.
    /// </summary>
    public int DaysToGracePeriod { get; private set; }

    /// <summary>
    /// Gets the foreign key to MarketIndex.
    /// </summary>
    public int MarketIndexId { get; private set; }

    /// <summary>
    /// Gets the foreign key to BondBase.
    /// </summary>
    public int BondBaseId { get; private set; }

    /// <summary>
    /// Gets the foreign key to BondEmitter.
    /// </summary>
    public int BondEmitterId { get; private set; }

    /// <summary>
    /// Gets the date and time when the bond detail was created.
    /// </summary>
    public DateTime CreatedAt { get; private init; }

    /// <summary>
    /// Gets the date and time when the bond detail was last updated.
    /// </summary>
    public DateTime? LastUpdatedAt { get; private set; }

    /// <summary>
    /// Gets whether this bond has daily liquidity (grace period is 0).
    /// </summary>
    public bool HasDailyLiquidity => DaysToGracePeriod == 0;

    /// <summary>
    /// Gets whether liquidity is only available at maturity.
    /// </summary>
    public bool LiquidityAtMaturityOnly => DaysToGracePeriod == DeadlineCalendarDays;

    /// <summary>
    /// Gets whether this is a pre-fixed bond (fixed rate > 0, benchmark = 0).
    /// </summary>
    public bool IsPreFixed => FixedPercentualRate.Value > 0 && BenchmarkPercentualRate.IsZero;

    /// <summary>
    /// Gets whether this is a post-fixed bond (benchmark rate > 0).
    /// </summary>
    public bool IsPostFixed => BenchmarkPercentualRate.Value > 0;

    /// <summary>
    /// Gets whether this is a hybrid bond (both fixed and benchmark rates > 0).
    /// </summary>
    public bool IsHybrid => FixedPercentualRate.Value > 0 && BenchmarkPercentualRate.Value > 0;

    /// <summary>
    /// Creates a new BondDetail instance (for new bond details not yet persisted).
    /// </summary>
    public static BondDetail Create(
        string? fantasyName,
        int deadlineCalendarDays,
        decimal initialUnitValue,
        decimal benchmarkPercentualRate,
        decimal fixedPercentualRate,
        bool isAvailable,
        bool isExemptDebenture,
        int daysToGracePeriod,
        int marketIndexId,
        int bondBaseId,
        int bondEmitterId)
    {
        if (deadlineCalendarDays <= 0)
        {
            throw new ArgumentException("Deadline calendar days must be greater than zero.", nameof(deadlineCalendarDays));
        }

        if (daysToGracePeriod < 0)
        {
            throw new ArgumentException("Days to grace period cannot be negative.", nameof(daysToGracePeriod));
        }

        if (daysToGracePeriod > deadlineCalendarDays)
        {
            throw new ArgumentException("Grace period cannot exceed deadline.", nameof(daysToGracePeriod));
        }

        if (marketIndexId <= 0)
        {
            throw new ArgumentException("Market index ID must be greater than zero.", nameof(marketIndexId));
        }

        if (bondBaseId <= 0)
        {
            throw new ArgumentException("Bond base ID must be greater than zero.", nameof(bondBaseId));
        }

        if (bondEmitterId <= 0)
        {
            throw new ArgumentException("Bond emitter ID must be greater than zero.", nameof(bondEmitterId));
        }

        return new BondDetail(
            id: 0, // Will be set by database
            fantasyName: fantasyName?.Trim(),
            deadlineCalendarDays: deadlineCalendarDays,
            initialUnitValue: Money.Create(initialUnitValue),
            benchmarkPercentualRate: Rate.Create(benchmarkPercentualRate),
            fixedPercentualRate: Rate.Create(fixedPercentualRate),
            isAvailable: isAvailable,
            isExemptDebenture: isExemptDebenture,
            daysToGracePeriod: daysToGracePeriod,
            marketIndexId: marketIndexId,
            bondBaseId: bondBaseId,
            bondEmitterId: bondEmitterId,
            createdAt: DateTime.UtcNow);
    }

    /// <summary>
    /// Reconstitutes a BondDetail from persistence (used by repository).
    /// </summary>
    public static BondDetail Reconstitute(
        int id,
        string? fantasyName,
        int deadlineCalendarDays,
        decimal initialUnitValue,
        decimal benchmarkPercentualRate,
        decimal fixedPercentualRate,
        bool isAvailable,
        bool isExemptDebenture,
        int daysToGracePeriod,
        int marketIndexId,
        int bondBaseId,
        int bondEmitterId,
        DateTime createdAt,
        DateTime? lastUpdatedAt)
    {
        var bondDetail = new BondDetail(
            id: id,
            fantasyName: fantasyName,
            deadlineCalendarDays: deadlineCalendarDays,
            initialUnitValue: Money.Create(initialUnitValue),
            benchmarkPercentualRate: Rate.Create(benchmarkPercentualRate),
            fixedPercentualRate: Rate.Create(fixedPercentualRate),
            isAvailable: isAvailable,
            isExemptDebenture: isExemptDebenture,
            daysToGracePeriod: daysToGracePeriod,
            marketIndexId: marketIndexId,
            bondBaseId: bondBaseId,
            bondEmitterId: bondEmitterId,
            createdAt: createdAt)
        {
            LastUpdatedAt = lastUpdatedAt
        };

        return bondDetail;
    }

    /// <summary>
    /// Updates the bond detail rates and configuration.
    /// </summary>
    public void UpdateRates(
        decimal benchmarkPercentualRate,
        decimal fixedPercentualRate,
        int daysToGracePeriod)
    {
        if (daysToGracePeriod < 0)
        {
            throw new ArgumentException("Days to grace period cannot be negative.", nameof(daysToGracePeriod));
        }

        if (daysToGracePeriod > DeadlineCalendarDays)
        {
            throw new ArgumentException("Grace period cannot exceed deadline.", nameof(daysToGracePeriod));
        }

        BenchmarkPercentualRate = Rate.Create(benchmarkPercentualRate);
        FixedPercentualRate = Rate.Create(fixedPercentualRate);
        DaysToGracePeriod = daysToGracePeriod;
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the initial unit value.
    /// </summary>
    public void UpdateInitialUnitValue(decimal newValue)
    {
        InitialUnitValue = Money.Create(newValue);
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the fantasy name.
    /// </summary>
    public void UpdateFantasyName(string? fantasyName)
    {
        FantasyName = fantasyName?.Trim();
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the availability status.
    /// </summary>
    public void UpdateAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the bond detail as available.
    /// </summary>
    public void MakeAvailable()
    {
        UpdateAvailability(true);
    }

    /// <summary>
    /// Marks the bond detail as unavailable.
    /// </summary>
    public void MakeUnavailable()
    {
        UpdateAvailability(false);
    }

    /// <summary>
    /// Gets the liquidity description for display.
    /// </summary>
    public string GetLiquidityDescription()
    {
        return DaysToGracePeriod switch
        {
            0 => "Diária",
            var value when value == DeadlineCalendarDays => "No Vencimento",
            _ => $"{DaysToGracePeriod} dias"
        };
    }

    /// <summary>
    /// Gets the deadline description for display.
    /// </summary>
    public string GetDeadlineDescription()
    {
        return DeadlineCalendarYears switch
        {
            < 1 => $"{DeadlineCalendarDays} dias",
            1 => "1 ano",
            _ => $"{DeadlineCalendarYears} anos"
        };
    }
}
