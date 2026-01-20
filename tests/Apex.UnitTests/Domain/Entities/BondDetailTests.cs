using Apex.Domain.Entities;

namespace Apex.UnitTests.Domain.Entities;

public class BondDetailTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateBondDetail()
    {
        // Act
        var bondDetail = BondDetail.Create(
            fantasyName: "Test Bond",
            deadlineCalendarDays: 720,
            initialUnitValue: 1000m,
            benchmarkPercentualRate: 2.5m,
            fixedPercentualRate: 10.5m,
            isAvailable: true,
            isExemptDebenture: false,
            daysToGracePeriod: 0,
            marketIndexId: 1,
            bondBaseId: 2,
            bondEmitterId: 3);

        // Assert
        Assert.NotNull(bondDetail);
        Assert.Equal(0, bondDetail.Id);
        Assert.Equal("Test Bond", bondDetail.FantasyName);
        Assert.Equal(720, bondDetail.DeadlineCalendarDays);
        Assert.Equal(2.0m, bondDetail.DeadlineCalendarYears);
        Assert.Equal(1000m, bondDetail.InitialUnitValue.Amount);
        Assert.Equal(2.5m, bondDetail.BenchmarkPercentualRate.Value);
        Assert.Equal(10.5m, bondDetail.FixedPercentualRate.Value);
        Assert.True(bondDetail.IsAvailable);
        Assert.False(bondDetail.IsExemptDebenture);
        Assert.Equal(0, bondDetail.DaysToGracePeriod);
        Assert.Equal(1, bondDetail.MarketIndexId);
        Assert.Equal(2, bondDetail.BondBaseId);
        Assert.Equal(3, bondDetail.BondEmitterId);
    }

    [Fact]
    public void Create_WithZeroDeadline_ShouldThrowException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => BondDetail.Create(
            fantasyName: "Test",
            deadlineCalendarDays: 0,
            initialUnitValue: 1000m,
            benchmarkPercentualRate: 2.5m,
            fixedPercentualRate: 0m,
            isAvailable: true,
            isExemptDebenture: false,
            daysToGracePeriod: 0,
            marketIndexId: 1,
            bondBaseId: 1,
            bondEmitterId: 1));

        Assert.Contains("must be greater than zero", exception.Message);
    }

    [Fact]
    public void Create_WithGracePeriodGreaterThanDeadline_ShouldThrowException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => BondDetail.Create(
            fantasyName: "Test",
            deadlineCalendarDays: 100,
            initialUnitValue: 1000m,
            benchmarkPercentualRate: 0m,
            fixedPercentualRate: 10m,
            isAvailable: true,
            isExemptDebenture: false,
            daysToGracePeriod: 150,
            marketIndexId: 1,
            bondBaseId: 1,
            bondEmitterId: 1));

        Assert.Contains("cannot exceed deadline", exception.Message);
    }

    [Fact]
    public void HasDailyLiquidity_WhenGracePeriodIsZero_ShouldReturnTrue()
    {
        // Arrange
        var bondDetail = BondDetail.Create(
            fantasyName: "Daily Liquidity Bond",
            deadlineCalendarDays: 365,
            initialUnitValue: 1000m,
            benchmarkPercentualRate: 1.5m,
            fixedPercentualRate: 0m,
            isAvailable: true,
            isExemptDebenture: false,
            daysToGracePeriod: 0,
            marketIndexId: 1,
            bondBaseId: 1,
            bondEmitterId: 1);

        // Assert
        Assert.True(bondDetail.HasDailyLiquidity);
        Assert.False(bondDetail.LiquidityAtMaturityOnly);
    }

    [Fact]
    public void LiquidityAtMaturityOnly_WhenGracePeriodEqualsDeadline_ShouldReturnTrue()
    {
        // Arrange
        var bondDetail = BondDetail.Create(
            fantasyName: "Maturity Only Bond",
            deadlineCalendarDays: 365,
            initialUnitValue: 1000m,
            benchmarkPercentualRate: 2m,
            fixedPercentualRate: 0m,
            isAvailable: true,
            isExemptDebenture: false,
            daysToGracePeriod: 365,
            marketIndexId: 1,
            bondBaseId: 1,
            bondEmitterId: 1);

        // Assert
        Assert.True(bondDetail.LiquidityAtMaturityOnly);
        Assert.False(bondDetail.HasDailyLiquidity);
    }

    [Fact]
    public void IsPreFixed_WhenOnlyFixedRateIsSet_ShouldReturnTrue()
    {
        // Arrange
        var bondDetail = BondDetail.Create(
            fantasyName: "Pre-fixed Bond",
            deadlineCalendarDays: 720,
            initialUnitValue: 1000m,
            benchmarkPercentualRate: 0m,
            fixedPercentualRate: 12.5m,
            isAvailable: true,
            isExemptDebenture: false,
            daysToGracePeriod: 0,
            marketIndexId: 1,
            bondBaseId: 1,
            bondEmitterId: 1);

        // Assert
        Assert.True(bondDetail.IsPreFixed);
        Assert.False(bondDetail.IsPostFixed);
        Assert.False(bondDetail.IsHybrid);
    }

    [Fact]
    public void IsPostFixed_WhenBenchmarkRateIsSet_ShouldReturnTrue()
    {
        // Arrange
        var bondDetail = BondDetail.Create(
            fantasyName: "Post-fixed Bond",
            deadlineCalendarDays: 360,
            initialUnitValue: 1000m,
            benchmarkPercentualRate: 2.0m,
            fixedPercentualRate: 0m,
            isAvailable: true,
            isExemptDebenture: false,
            daysToGracePeriod: 0,
            marketIndexId: 1,
            bondBaseId: 1,
            bondEmitterId: 1);

        // Assert
        Assert.True(bondDetail.IsPostFixed);
        Assert.False(bondDetail.IsPreFixed);
        Assert.False(bondDetail.IsHybrid);
    }

    [Fact]
    public void IsHybrid_WhenBothRatesAreSet_ShouldReturnTrue()
    {
        // Arrange
        var bondDetail = BondDetail.Create(
            fantasyName: "Hybrid Bond",
            deadlineCalendarDays: 720,
            initialUnitValue: 1000m,
            benchmarkPercentualRate: 2.5m,
            fixedPercentualRate: 8.0m,
            isAvailable: true,
            isExemptDebenture: false,
            daysToGracePeriod: 30,
            marketIndexId: 1,
            bondBaseId: 1,
            bondEmitterId: 1);

        // Assert
        Assert.True(bondDetail.IsHybrid);
        Assert.True(bondDetail.IsPostFixed); // Hybrid is also post-fixed
        Assert.False(bondDetail.IsPreFixed);
    }

    [Fact]
    public void UpdateRates_WithValidParameters_ShouldUpdateRates()
    {
        // Arrange
        var bondDetail = BondDetail.Create(
            fantasyName: "Test",
            deadlineCalendarDays: 360,
            initialUnitValue: 1000m,
            benchmarkPercentualRate: 2m,
            fixedPercentualRate: 10m,
            isAvailable: true,
            isExemptDebenture: false,
            daysToGracePeriod: 0,
            marketIndexId: 1,
            bondBaseId: 1,
            bondEmitterId: 1);

        // Act
        bondDetail.UpdateRates(
            benchmarkPercentualRate: 3.5m,
            fixedPercentualRate: 12m,
            daysToGracePeriod: 30);

        // Assert
        Assert.Equal(3.5m, bondDetail.BenchmarkPercentualRate.Value);
        Assert.Equal(12m, bondDetail.FixedPercentualRate.Value);
        Assert.Equal(30, bondDetail.DaysToGracePeriod);
        Assert.NotNull(bondDetail.LastUpdatedAt);
    }

    [Fact]
    public void UpdateInitialUnitValue_ShouldUpdateValue()
    {
        // Arrange
        var bondDetail = BondDetail.Create(
            fantasyName: "Test",
            deadlineCalendarDays: 360,
            initialUnitValue: 1000m,
            benchmarkPercentualRate: 0m,
            fixedPercentualRate: 10m,
            isAvailable: true,
            isExemptDebenture: false,
            daysToGracePeriod: 0,
            marketIndexId: 1,
            bondBaseId: 1,
            bondEmitterId: 1);

        // Act
        bondDetail.UpdateInitialUnitValue(1500m);

        // Assert
        Assert.Equal(1500m, bondDetail.InitialUnitValue.Amount);
        Assert.NotNull(bondDetail.LastUpdatedAt);
    }

    [Fact]
    public void MakeAvailable_ShouldSetAvailabilityToTrue()
    {
        // Arrange
        var bondDetail = BondDetail.Create(
            fantasyName: "Test",
            deadlineCalendarDays: 360,
            initialUnitValue: 1000m,
            benchmarkPercentualRate: 0m,
            fixedPercentualRate: 10m,
            isAvailable: false,
            isExemptDebenture: false,
            daysToGracePeriod: 0,
            marketIndexId: 1,
            bondBaseId: 1,
            bondEmitterId: 1);

        // Act
        bondDetail.MakeAvailable();

        // Assert
        Assert.True(bondDetail.IsAvailable);
    }

    [Fact]
    public void MakeUnavailable_ShouldSetAvailabilityToFalse()
    {
        // Arrange
        var bondDetail = BondDetail.Create(
            fantasyName: "Test",
            deadlineCalendarDays: 360,
            initialUnitValue: 1000m,
            benchmarkPercentualRate: 0m,
            fixedPercentualRate: 10m,
            isAvailable: true,
            isExemptDebenture: false,
            daysToGracePeriod: 0,
            marketIndexId: 1,
            bondBaseId: 1,
            bondEmitterId: 1);

        // Act
        bondDetail.MakeUnavailable();

        // Assert
        Assert.False(bondDetail.IsAvailable);
    }

    [Fact]
    public void GetLiquidityDescription_WithDailyLiquidity_ShouldReturnDiaria()
    {
        // Arrange
        var bondDetail = BondDetail.Create(
            fantasyName: "Test",
            deadlineCalendarDays: 360,
            initialUnitValue: 1000m,
            benchmarkPercentualRate: 2m,
            fixedPercentualRate: 0m,
            isAvailable: true,
            isExemptDebenture: false,
            daysToGracePeriod: 0,
            marketIndexId: 1,
            bondBaseId: 1,
            bondEmitterId: 1);

        // Assert
        Assert.Equal("Diária", bondDetail.GetLiquidityDescription());
    }

    [Fact]
    public void GetLiquidityDescription_WithMaturityOnly_ShouldReturnNoVencimento()
    {
        // Arrange
        var bondDetail = BondDetail.Create(
            fantasyName: "Test",
            deadlineCalendarDays: 365,
            initialUnitValue: 1000m,
            benchmarkPercentualRate: 2m,
            fixedPercentualRate: 0m,
            isAvailable: true,
            isExemptDebenture: false,
            daysToGracePeriod: 365,
            marketIndexId: 1,
            bondBaseId: 1,
            bondEmitterId: 1);

        // Assert
        Assert.Equal("No Vencimento", bondDetail.GetLiquidityDescription());
    }

    [Fact]
    public void GetLiquidityDescription_WithCustomPeriod_ShouldReturnDays()
    {
        // Arrange
        var bondDetail = BondDetail.Create(
            fantasyName: "Test",
            deadlineCalendarDays: 720,
            initialUnitValue: 1000m,
            benchmarkPercentualRate: 2m,
            fixedPercentualRate: 0m,
            isAvailable: true,
            isExemptDebenture: false,
            daysToGracePeriod: 90,
            marketIndexId: 1,
            bondBaseId: 1,
            bondEmitterId: 1);

        // Assert
        Assert.Equal("90 dias", bondDetail.GetLiquidityDescription());
    }

    [Fact]
    public void GetDeadlineDescription_WithLessThanOneYear_ShouldReturnDays()
    {
        // Arrange
        var bondDetail = BondDetail.Create(
            fantasyName: "Test",
            deadlineCalendarDays: 180,
            initialUnitValue: 1000m,
            benchmarkPercentualRate: 2m,
            fixedPercentualRate: 0m,
            isAvailable: true,
            isExemptDebenture: false,
            daysToGracePeriod: 0,
            marketIndexId: 1,
            bondBaseId: 1,
            bondEmitterId: 1);

        // Assert
        Assert.Equal("180 dias", bondDetail.GetDeadlineDescription());
    }

    [Fact]
    public void GetDeadlineDescription_WithExactlyOneYear_ShouldReturn1Ano()
    {
        // Arrange
        var bondDetail = BondDetail.Create(
            fantasyName: "Test",
            deadlineCalendarDays: 360,
            initialUnitValue: 1000m,
            benchmarkPercentualRate: 2m,
            fixedPercentualRate: 0m,
            isAvailable: true,
            isExemptDebenture: false,
            daysToGracePeriod: 0,
            marketIndexId: 1,
            bondBaseId: 1,
            bondEmitterId: 1);

        // Assert
        Assert.Equal("1 ano", bondDetail.GetDeadlineDescription());
    }

    [Fact]
    public void GetDeadlineDescription_WithMultipleYears_ShouldReturnYears()
    {
        // Arrange
        var bondDetail = BondDetail.Create(
            fantasyName: "Test",
            deadlineCalendarDays: 720,
            initialUnitValue: 1000m,
            benchmarkPercentualRate: 2m,
            fixedPercentualRate: 0m,
            isAvailable: true,
            isExemptDebenture: false,
            daysToGracePeriod: 0,
            marketIndexId: 1,
            bondBaseId: 1,
            bondEmitterId: 1);

        // Assert
        Assert.Equal("2 anos", bondDetail.GetDeadlineDescription());
    }
}
