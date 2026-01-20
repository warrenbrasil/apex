using Apex.Domain.Entities;
using Apex.Domain.Exceptions;

namespace Apex.UnitTests.Domain.Entities;

public class BondTests
{
    private const string ValidSymbol = "CDB123";
    private const string ValidIsin = "BRXYZ1234567";
    private readonly DateTime _issuanceDate = DateTime.UtcNow.Date;
    private readonly DateTime _expirationDate = DateTime.UtcNow.AddYears(2).Date;

    [Fact]
    public void Create_WithValidParameters_ShouldCreateBond()
    {
        // Act
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            _issuanceDate,
            _expirationDate);

        // Assert
        Assert.NotNull(bond);
        Assert.Equal(0, bond.Id); // ID is 0 until persisted
        Assert.False(bond.ExistsInDatabase);
        Assert.NotEqual(Guid.Empty, bond.ApiId);
        Assert.Equal(ValidSymbol, bond.Symbol.Value);
        Assert.Equal(ValidIsin, bond.Isin.Value);
        Assert.Equal(_issuanceDate, bond.IssuanceAt);
        Assert.Equal(_expirationDate, bond.ExpirationAt);
        Assert.False(bond.IsCetipVerified);
        Assert.Null(bond.BondDetailId);
        Assert.True(bond.CreatedAt <= DateTime.UtcNow);
        Assert.Null(bond.LastUpdatedAt);
    }

    [Fact]
    public void Create_WithBondDetailId_ShouldCreateBondWithDetail()
    {
        // Arrange
        const int bondDetailId = 123;

        // Act
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            _issuanceDate,
            _expirationDate,
            bondDetailId);

        // Assert
        Assert.Equal(bondDetailId, bond.BondDetailId);
        Assert.True(bond.HasBondDetail);
    }

    [Fact]
    public void Create_WithCetipVerified_ShouldCreateVerifiedBond()
    {
        // Act
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            _issuanceDate,
            _expirationDate,
            isCetipVerified: true);

        // Assert
        Assert.True(bond.IsCetipVerified);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidSymbol_ShouldThrowException(string invalidSymbol)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Bond.Create(
            invalidSymbol,
            ValidIsin,
            _issuanceDate,
            _expirationDate));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("INVALID")]  // Less than 12 characters
    [InlineData("BRXYZ12345678")] // More than 12 characters
    [InlineData("12XYZ1234567")] // Doesn't start with 2 letters
    public void Create_WithInvalidIsin_ShouldThrowException(string invalidIsin)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Bond.Create(
            ValidSymbol,
            invalidIsin,
            _issuanceDate,
            _expirationDate));
    }

    [Fact]
    public void Create_WithExpirationBeforeIssuance_ShouldThrowException()
    {
        // Arrange
        var issuance = DateTime.UtcNow.Date;
        var expiration = issuance.AddDays(-1);

        // Act & Assert
        var exception = Assert.Throws<InvalidBondException>(() => Bond.Create(
            ValidSymbol,
            ValidIsin,
            issuance,
            expiration));

        Assert.Contains("after issuance date", exception.Message);
    }

    [Fact]
    public void Create_WithExpirationEqualToIssuance_ShouldThrowException()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;

        // Act & Assert
        var exception = Assert.Throws<InvalidBondException>(() => Bond.Create(
            ValidSymbol,
            ValidIsin,
            date,
            date));

        Assert.Contains("after issuance date", exception.Message);
    }

    [Fact]
    public void HasExpired_WhenExpirationInFuture_ShouldReturnFalse()
    {
        // Arrange
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(30));

        // Assert
        Assert.False(bond.HasExpired);
    }

    [Fact]
    public void HasExpired_WhenExpirationInPast_ShouldReturnTrue()
    {
        // Arrange
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            DateTime.UtcNow.AddDays(-365),
            DateTime.UtcNow.AddDays(-1));

        // Assert
        Assert.True(bond.HasExpired);
    }

    [Fact]
    public void IsActive_WhenWithinDateRange_ShouldReturnTrue()
    {
        // Arrange
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(30));

        // Assert
        Assert.True(bond.IsActive);
    }

    [Fact]
    public void IsActive_WhenIssuanceInFuture_ShouldReturnFalse()
    {
        // Arrange
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            DateTime.UtcNow.AddDays(10),
            DateTime.UtcNow.AddDays(100));

        // Assert
        Assert.False(bond.IsActive);
    }

    [Fact]
    public void IsActive_WhenExpired_ShouldReturnFalse()
    {
        // Arrange
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            DateTime.UtcNow.AddDays(-365),
            DateTime.UtcNow.AddDays(-1));

        // Assert
        Assert.False(bond.IsActive);
    }

    [Fact]
    public void RemainingDays_WhenNotExpired_ShouldReturnCorrectValue()
    {
        // Arrange
        var expirationDate = DateTime.UtcNow.AddDays(30).Date;
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            DateTime.UtcNow.AddDays(-30),
            expirationDate);

        // Act
        var remainingDays = bond.RemainingDays;

        // Assert
        Assert.Equal(30, remainingDays);
    }

    [Fact]
    public void RemainingDays_WhenExpired_ShouldReturnZero()
    {
        // Arrange
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            DateTime.UtcNow.AddDays(-365),
            DateTime.UtcNow.AddDays(-1));

        // Assert
        Assert.Equal(0, bond.RemainingDays);
    }

    [Fact]
    public void DurationInCalendarDays_ShouldCalculateCorrectly()
    {
        // Arrange
        var issuance = new DateTime(2024, 1, 1);
        var expiration = new DateTime(2026, 1, 1);
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            issuance,
            expiration);

        // Act
        var duration = bond.DurationInCalendarDays;

        // Assert
        Assert.Equal(731, duration); // 2 years = 731 days (including leap year)
    }

    [Fact]
    public void DurationInYears_ShouldUse360DayConvention()
    {
        // Arrange
        var issuance = new DateTime(2024, 1, 1);
        var expiration = new DateTime(2026, 1, 1);
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            issuance,
            expiration);

        // Act
        var durationInYears = bond.DurationInYears;

        // Assert
        Assert.Equal(731m / 360m, durationInYears);
    }

    [Fact]
    public void UpdateCetipVerification_ShouldUpdateStatus()
    {
        // Arrange
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            _issuanceDate,
            _expirationDate);

        // Act
        bond.UpdateCetipVerification(true);

        // Assert
        Assert.True(bond.IsCetipVerified);
        Assert.NotNull(bond.LastUpdatedAt);
    }

    [Fact]
    public void LinkToBondDetail_WithValidId_ShouldLinkBond()
    {
        // Arrange
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            _issuanceDate,
            _expirationDate);
        const int bondDetailId = 123;

        // Act
        bond.LinkToBondDetail(bondDetailId);

        // Assert
        Assert.Equal(bondDetailId, bond.BondDetailId);
        Assert.True(bond.HasBondDetail);
        Assert.NotNull(bond.LastUpdatedAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void LinkToBondDetail_WithInvalidId_ShouldThrowException(int invalidId)
    {
        // Arrange
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            _issuanceDate,
            _expirationDate);

        // Act & Assert
        var exception = Assert.Throws<InvalidBondException>(() => bond.LinkToBondDetail(invalidId));
        Assert.Contains("Invalid BondDetailId", exception.Message);
    }

    [Fact]
    public void UnlinkFromBondDetail_ShouldRemoveLink()
    {
        // Arrange
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            _issuanceDate,
            _expirationDate,
            bondDetailId: 123);

        // Act
        bond.UnlinkFromBondDetail();

        // Assert
        Assert.Null(bond.BondDetailId);
        Assert.False(bond.HasBondDetail);
        Assert.NotNull(bond.LastUpdatedAt);
    }

    [Fact]
    public void ExtendExpiration_WithValidDate_ShouldExtendBond()
    {
        // Arrange
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            _issuanceDate,
            _expirationDate);
        var newExpiration = _expirationDate.AddYears(1);

        // Act
        bond.ExtendExpiration(newExpiration);

        // Assert
        Assert.Equal(newExpiration, bond.ExpirationAt);
        Assert.NotNull(bond.LastUpdatedAt);
    }

    [Fact]
    public void ExtendExpiration_WithDateBeforeCurrent_ShouldThrowException()
    {
        // Arrange
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            _issuanceDate,
            _expirationDate);
        var invalidDate = _expirationDate.AddDays(-1);

        // Act & Assert
        var exception = Assert.Throws<InvalidBondException>(() => bond.ExtendExpiration(invalidDate));
        Assert.Contains("after current expiration date", exception.Message);
    }

    [Fact]
    public void ExtendExpiration_WithDateBeforeIssuance_ShouldThrowException()
    {
        // Arrange
        var issuance = DateTime.UtcNow.Date;
        var expiration = DateTime.UtcNow.AddDays(30).Date;
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            issuance,
            expiration);

        // Try to extend to a date that's after current expiration but before issuance
        // This scenario shouldn't happen in practice, but we test the validation
        var invalidDate = issuance.AddDays(-1);

        // Act & Assert - Will fail on "after current expiration" check first
        var exception = Assert.Throws<InvalidBondException>(() => bond.ExtendExpiration(invalidDate));
        Assert.Contains("after current expiration date", exception.Message);
    }

    [Fact]
    public void UpdateSymbol_WithValidSymbol_ShouldUpdateBond()
    {
        // Arrange
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            _issuanceDate,
            _expirationDate);
        const string newSymbol = "NEW123";

        // Act
        bond.UpdateSymbol(newSymbol);

        // Assert
        Assert.Equal(newSymbol, bond.Symbol.Value);
        Assert.NotNull(bond.LastUpdatedAt);
    }

    [Fact]
    public void UpdateIsin_WithValidIsin_ShouldUpdateBond()
    {
        // Arrange
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            _issuanceDate,
            _expirationDate);
        const string newIsin = "BRABC9876543";

        // Act
        bond.UpdateIsin(newIsin);

        // Assert
        Assert.Equal(newIsin, bond.Isin.Value);
        Assert.NotNull(bond.LastUpdatedAt);
    }

    [Fact]
    public void EnsureIsActive_WhenActive_ShouldNotThrow()
    {
        // Arrange
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(30));

        // Act & Assert - Should not throw
        bond.EnsureIsActive();
    }

    [Fact]
    public void EnsureIsActive_WhenNotActive_ShouldThrowException()
    {
        // Arrange
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            DateTime.UtcNow.AddDays(-365),
            DateTime.UtcNow.AddDays(-1));

        // Act & Assert
        Assert.Throws<BondNotActiveException>(() => bond.EnsureIsActive());
    }

    [Fact]
    public void EnsureNotExpired_WhenNotExpired_ShouldNotThrow()
    {
        // Arrange
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(30));

        // Act & Assert - Should not throw
        bond.EnsureNotExpired();
    }

    [Fact]
    public void EnsureNotExpired_WhenExpired_ShouldThrowException()
    {
        // Arrange
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            DateTime.UtcNow.AddDays(-365),
            DateTime.UtcNow.AddDays(-1));

        // Act & Assert
        Assert.Throws<BondExpiredException>(() => bond.EnsureNotExpired());
    }

    [Fact]
    public void EnsureCetipVerified_WhenVerified_ShouldNotThrow()
    {
        // Arrange
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            _issuanceDate,
            _expirationDate,
            isCetipVerified: true);

        // Act & Assert - Should not throw
        bond.EnsureCetipVerified();
    }

    [Fact]
    public void EnsureCetipVerified_WhenNotVerified_ShouldThrowException()
    {
        // Arrange
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            _issuanceDate,
            _expirationDate);

        // Act & Assert
        Assert.Throws<CetipVerificationException>(() => bond.EnsureCetipVerified());
    }

    [Fact]
    public void Reconstitute_ShouldRestoreBondFromPersistence()
    {
        // Arrange
        const int id = 12345;
        var apiId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-100);
        var lastUpdatedAt = DateTime.UtcNow.AddDays(-1);
        const int bondDetailId = 456;

        // Act
        var bond = Bond.Reconstitute(
            id,
            ValidSymbol,
            ValidIsin,
            _issuanceDate,
            _expirationDate,
            bondDetailId,
            isCetipVerified: true,
            apiId,
            createdAt,
            lastUpdatedAt);

        // Assert
        Assert.Equal(id, bond.Id);
        Assert.True(bond.ExistsInDatabase);
        Assert.Equal(apiId, bond.ApiId);
        Assert.Equal(ValidSymbol, bond.Symbol.Value);
        Assert.Equal(ValidIsin, bond.Isin.Value);
        Assert.Equal(bondDetailId, bond.BondDetailId);
        Assert.True(bond.IsCetipVerified);
        Assert.Equal(createdAt, bond.CreatedAt);
        Assert.Equal(lastUpdatedAt, bond.LastUpdatedAt);
    }

    [Fact]
    public void ExistsInDatabase_WhenIdIsZero_ShouldReturnFalse()
    {
        // Arrange
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            _issuanceDate,
            _expirationDate);

        // Assert
        Assert.False(bond.ExistsInDatabase);
    }

    [Fact]
    public void ExistsInDatabase_WhenIdIsGreaterThanZero_ShouldReturnTrue()
    {
        // Arrange
        var bond = Bond.Reconstitute(
            id: 123,
            ValidSymbol,
            ValidIsin,
            _issuanceDate,
            _expirationDate,
            bondDetailId: null,
            isCetipVerified: false,
            apiId: Guid.NewGuid(),
            createdAt: DateTime.UtcNow,
            lastUpdatedAt: null);

        // Assert
        Assert.True(bond.ExistsInDatabase);
    }

    [Fact]
    public void Create_WithCustomApiId_ShouldUseProvidedApiId()
    {
        // Arrange
        var customApiId = Guid.NewGuid();

        // Act
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            _issuanceDate,
            _expirationDate,
            apiId: customApiId);

        // Assert
        Assert.Equal(customApiId, bond.ApiId);
    }

    [Fact]
    public void UpdateApiId_WithValidGuid_ShouldUpdateApiId()
    {
        // Arrange
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            _issuanceDate,
            _expirationDate);
        var newApiId = Guid.NewGuid();

        // Act
        bond.UpdateApiId(newApiId);

        // Assert
        Assert.Equal(newApiId, bond.ApiId);
        Assert.NotNull(bond.LastUpdatedAt);
    }

    [Fact]
    public void UpdateApiId_WithEmptyGuid_ShouldThrowException()
    {
        // Arrange
        var bond = Bond.Create(
            ValidSymbol,
            ValidIsin,
            _issuanceDate,
            _expirationDate);

        // Act & Assert
        var exception = Assert.Throws<InvalidBondException>(() => bond.UpdateApiId(Guid.Empty));
        Assert.Contains("API ID cannot be empty", exception.Message);
    }
}
