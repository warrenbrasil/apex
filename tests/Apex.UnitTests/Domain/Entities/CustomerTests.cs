using Apex.Domain.Entities;
using Apex.Domain.Enums;
using Apex.Domain.Exceptions;

namespace Apex.UnitTests.Domain.Entities;

public class CustomerTests
{
    private const string ValidApiId = "API123456";
    private const string ValidCpf = "12345678901";
    private const string ValidCnpj = "12345678000190";
    private const string ValidSinacorId = "123456789";

    [Fact]
    public void Create_WithValidParameters_ShouldCreateCustomer()
    {
        // Act
        var customer = Customer.Create(
            apiId: ValidApiId,
            document: ValidCpf,
            company: Company.Warren);

        // Assert
        Assert.NotNull(customer);
        Assert.Equal(0, customer.Id);
        Assert.Equal(ValidApiId, customer.ApiId);
        Assert.Equal(ValidCpf, customer.Document.Value);
        Assert.True(customer.Document.IsCpf);
        Assert.Equal(Company.Warren, customer.Company);
        Assert.Null(customer.SinacorId);
        Assert.Null(customer.LegacyExternalId);
        Assert.True(customer.CreatedAt <= DateTime.UtcNow);
        Assert.Null(customer.LastUpdatedAt);
    }

    [Fact]
    public void Create_ShouldInitializeExternalRegisters()
    {
        // Act
        var customer = Customer.Create(
            apiId: ValidApiId,
            document: ValidCpf,
            company: Company.Warren);

        // Assert
        Assert.Equal(2, customer.ExternalRegisters.Count);
        Assert.Contains(customer.ExternalRegisters, r => r.SystemType == CustomerExternalSystemType.Cetip);
        Assert.Contains(customer.ExternalRegisters, r => r.SystemType == CustomerExternalSystemType.Selic);
        Assert.All(customer.ExternalRegisters, r => Assert.Equal(CustomerExternalSystemStatus.NotRegistered, r.Status));
    }

    [Fact]
    public void Create_WithCpf_ShouldBeIndividual()
    {
        // Act
        var customer = Customer.Create(
            apiId: ValidApiId,
            document: ValidCpf,
            company: Company.Warren);

        // Assert
        Assert.True(customer.IsIndividual);
        Assert.False(customer.IsLegalEntity);
    }

    [Fact]
    public void Create_WithCnpj_ShouldBeLegalEntity()
    {
        // Act
        var customer = Customer.Create(
            apiId: ValidApiId,
            document: ValidCnpj,
            company: Company.Warren);

        // Assert
        Assert.True(customer.IsLegalEntity);
        Assert.False(customer.IsIndividual);
    }

    [Fact]
    public void Create_WithSinacorId_ShouldSetSinacorId()
    {
        // Act
        var customer = Customer.Create(
            apiId: ValidApiId,
            document: ValidCpf,
            company: Company.Warren,
            sinacorId: ValidSinacorId);

        // Assert
        Assert.Equal(ValidSinacorId, customer.SinacorId);
        Assert.True(customer.HasSinacorId);
    }

    [Fact]
    public void Create_WithLegacyExternalId_ShouldSetLegacyExternalId()
    {
        // Arrange
        const string legacyId = "LEGACY123";

        // Act
        var customer = Customer.Create(
            apiId: ValidApiId,
            document: ValidCpf,
            company: Company.Warren,
            legacyExternalId: legacyId);

        // Assert
        Assert.Equal(legacyId, customer.LegacyExternalId);
        Assert.True(customer.HasLegacyExternalId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidApiId_ShouldThrowException(string invalidApiId)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Customer.Create(
            apiId: invalidApiId,
            document: ValidCpf,
            company: Company.Warren));
    }

    [Fact]
    public void Create_WithApiIdTooLong_ShouldThrowException()
    {
        // Arrange
        var longApiId = new string('A', 33); // 33 characters

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Customer.Create(
            apiId: longApiId,
            document: ValidCpf,
            company: Company.Warren));

        Assert.Contains("32 characters", exception.Message);
    }

    [Fact]
    public void Create_WithSinacorIdTooLong_ShouldThrowException()
    {
        // Arrange
        var longSinacorId = new string('1', 10); // 10 characters

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Customer.Create(
            apiId: ValidApiId,
            document: ValidCpf,
            company: Company.Warren,
            sinacorId: longSinacorId));

        Assert.Contains("9 characters", exception.Message);
    }

    [Fact]
    public void IsWarrenCustomer_WhenCompanyIsWarren_ShouldReturnTrue()
    {
        // Arrange
        var customer = Customer.Create(
            apiId: ValidApiId,
            document: ValidCpf,
            company: Company.Warren);

        // Assert
        Assert.True(customer.IsWarrenCustomer);
        Assert.False(customer.IsRenaCustomer);
    }

    [Fact]
    public void IsRenaCustomer_WhenCompanyIsRena_ShouldReturnTrue()
    {
        // Arrange
        var customer = Customer.Create(
            apiId: ValidApiId,
            document: ValidCpf,
            company: Company.Rena);

        // Assert
        Assert.True(customer.IsRenaCustomer);
        Assert.False(customer.IsWarrenCustomer);
    }

    [Fact]
    public void UpdateApiId_WithValidId_ShouldUpdateApiId()
    {
        // Arrange
        var customer = Customer.Create(
            apiId: ValidApiId,
            document: ValidCpf,
            company: Company.Warren);
        const string newApiId = "NEWAPI123";

        // Act
        customer.UpdateApiId(newApiId);

        // Assert
        Assert.Equal(newApiId, customer.ApiId);
        Assert.NotNull(customer.LastUpdatedAt);
    }

    [Fact]
    public void UpdateSinacorId_WithValidId_ShouldUpdateSinacorId()
    {
        // Arrange
        var customer = Customer.Create(
            apiId: ValidApiId,
            document: ValidCpf,
            company: Company.Warren);
        const string newSinacorId = "987654321";

        // Act
        customer.UpdateSinacorId(newSinacorId);

        // Assert
        Assert.Equal(newSinacorId, customer.SinacorId);
        Assert.True(customer.HasSinacorId);
        Assert.NotNull(customer.LastUpdatedAt);
    }

    [Fact]
    public void UpdateCompany_ShouldUpdateCompany()
    {
        // Arrange
        var customer = Customer.Create(
            apiId: ValidApiId,
            document: ValidCpf,
            company: Company.Warren);

        // Act
        customer.UpdateCompany(Company.Rena);

        // Assert
        Assert.Equal(Company.Rena, customer.Company);
        Assert.True(customer.IsRenaCustomer);
        Assert.NotNull(customer.LastUpdatedAt);
    }

    [Fact]
    public void UpdateDocument_WithValidDocument_ShouldUpdateDocument()
    {
        // Arrange
        var customer = Customer.Create(
            apiId: ValidApiId,
            document: ValidCpf,
            company: Company.Warren);

        // Act
        customer.UpdateDocument(ValidCnpj);

        // Assert
        Assert.Equal(ValidCnpj, customer.Document.Value);
        Assert.True(customer.IsLegalEntity);
        Assert.NotNull(customer.LastUpdatedAt);
    }

    [Fact]
    public void GetCetipRegister_ShouldReturnCetipRegister()
    {
        // Arrange
        var customer = Customer.Create(
            apiId: ValidApiId,
            document: ValidCpf,
            company: Company.Warren);

        // Act
        var cetipRegister = customer.GetCetipRegister();

        // Assert
        Assert.NotNull(cetipRegister);
        Assert.Equal(CustomerExternalSystemType.Cetip, cetipRegister.SystemType);
    }

    [Fact]
    public void GetSelicRegister_ShouldReturnSelicRegister()
    {
        // Arrange
        var customer = Customer.Create(
            apiId: ValidApiId,
            document: ValidCpf,
            company: Company.Warren);

        // Act
        var selicRegister = customer.GetSelicRegister();

        // Assert
        Assert.NotNull(selicRegister);
        Assert.Equal(CustomerExternalSystemType.Selic, selicRegister.SystemType);
    }

    [Fact]
    public void IsRegisteredInCetip_WhenNotRegistered_ShouldReturnFalse()
    {
        // Arrange
        var customer = Customer.Create(
            apiId: ValidApiId,
            document: ValidCpf,
            company: Company.Warren);

        // Assert
        Assert.False(customer.IsRegisteredInCetip());
    }

    [Fact]
    public void MarkAsRegisteredIn_ShouldUpdateRegisterStatus()
    {
        // Arrange
        var customer = Customer.Create(
            apiId: ValidApiId,
            document: ValidCpf,
            company: Company.Warren);

        // Act
        customer.MarkAsRegisteredIn(CustomerExternalSystemType.Cetip);

        // Assert
        Assert.True(customer.IsRegisteredInCetip());
        var cetipRegister = customer.GetCetipRegister();
        Assert.NotNull(cetipRegister);
        Assert.Equal(CustomerExternalSystemStatus.Registered, cetipRegister.Status);
        Assert.NotNull(customer.LastUpdatedAt);
    }

    [Fact]
    public void MarkAsInactiveIn_ShouldUpdateRegisterStatus()
    {
        // Arrange
        var customer = Customer.Create(
            apiId: ValidApiId,
            document: ValidCpf,
            company: Company.Warren);
        customer.MarkAsRegisteredIn(CustomerExternalSystemType.Selic);

        // Act
        customer.MarkAsInactiveIn(CustomerExternalSystemType.Selic);

        // Assert
        var selicRegister = customer.GetSelicRegister();
        Assert.NotNull(selicRegister);
        Assert.Equal(CustomerExternalSystemStatus.Inactive, selicRegister.Status);
        Assert.True(selicRegister.IsInactive);
    }

    [Fact]
    public void Reconstitute_ShouldRestoreCustomerFromPersistence()
    {
        // Arrange
        const int id = 12345;
        var createdAt = DateTime.UtcNow.AddDays(-100);
        var lastUpdatedAt = DateTime.UtcNow.AddDays(-1);

        // Act
        var customer = Customer.Reconstitute(
            id: id,
            apiId: ValidApiId,
            document: ValidCpf,
            sinacorId: ValidSinacorId,
            company: Company.Warren,
            legacyExternalId: "LEGACY123",
            createdAt: createdAt,
            lastUpdatedAt: lastUpdatedAt);

        // Assert
        Assert.Equal(id, customer.Id);
        Assert.Equal(ValidApiId, customer.ApiId);
        Assert.Equal(ValidCpf, customer.Document.Value);
        Assert.Equal(ValidSinacorId, customer.SinacorId);
        Assert.Equal(Company.Warren, customer.Company);
        Assert.Equal("LEGACY123", customer.LegacyExternalId);
        Assert.Equal(createdAt, customer.CreatedAt);
        Assert.Equal(lastUpdatedAt, customer.LastUpdatedAt);
    }

    [Fact]
    public void Reconstitute_WithExternalRegisters_ShouldIncludeRegisters()
    {
        // Arrange
        var registers = new List<CustomerExternalSystemRegister>
        {
            CustomerExternalSystemRegister.Reconstitute(
                id: 1,
                status: CustomerExternalSystemStatus.Registered,
                systemType: CustomerExternalSystemType.Cetip,
                customerId: 12345,
                createdAt: DateTime.UtcNow,
                lastUpdatedAt: null),
            CustomerExternalSystemRegister.Reconstitute(
                id: 2,
                status: CustomerExternalSystemStatus.NotRegistered,
                systemType: CustomerExternalSystemType.Selic,
                customerId: 12345,
                createdAt: DateTime.UtcNow,
                lastUpdatedAt: null)
        };

        // Act
        var customer = Customer.Reconstitute(
            id: 12345,
            apiId: ValidApiId,
            document: ValidCpf,
            sinacorId: null,
            company: Company.Warren,
            legacyExternalId: null,
            createdAt: DateTime.UtcNow,
            lastUpdatedAt: null,
            externalRegisters: registers);

        // Assert
        Assert.Equal(2, customer.ExternalRegisters.Count);
        Assert.True(customer.IsRegisteredInCetip());
        Assert.False(customer.IsRegisteredInSelic());
    }
}
