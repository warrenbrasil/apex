using Apex.Application.Customers.Queries.GetCustomer;
using Apex.Domain.Entities;
using Apex.Domain.Enums;
using Apex.Domain.Repositories;
using NSubstitute;

namespace Apex.UnitTests.Application.Customers.Queries;

public class GetCustomerQueryHandlerTests
{
    private readonly ICustomerRepository _customerRepository;
    private readonly GetCustomerQueryHandler _handler;

    public GetCustomerQueryHandlerTests()
    {
        _customerRepository = Substitute.For<ICustomerRepository>();
        _handler = new GetCustomerQueryHandler(_customerRepository);
    }

    [Fact]
    public async Task HandleAsync_WithValidId_ShouldReturnCustomer()
    {
        // Arrange
        var customer = Customer.Create(
            apiId: "API123",
            document: "12345678901",
            company: Company.Warren,
            sinacorId: "123456789");

        var reconstituted = Customer.Reconstitute(
            id: 1,
            apiId: customer.ApiId,
            document: customer.Document.Value,
            sinacorId: customer.SinacorId,
            company: customer.Company,
            legacyExternalId: customer.LegacyExternalId,
            createdAt: customer.CreatedAt,
            lastUpdatedAt: customer.LastUpdatedAt,
            externalRegisters: customer.ExternalRegisters.ToList());

        _customerRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(reconstituted);

        var query = new GetCustomerQuery(Id: 1);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.Id);
        Assert.Equal("API123", result.Value.ApiId);
        Assert.Equal("12345678901", result.Value.Document);
        Assert.Equal("Warren", result.Value.Company);

        await _customerRepository.Received(1).GetByIdAsync(1, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithValidApiId_ShouldReturnCustomer()
    {
        // Arrange
        var customer = Customer.Create(
            apiId: "API123",
            document: "12345678901",
            company: Company.Warren);

        var reconstituted = Customer.Reconstitute(
            id: 1,
            apiId: customer.ApiId,
            document: customer.Document.Value,
            sinacorId: customer.SinacorId,
            company: customer.Company,
            legacyExternalId: customer.LegacyExternalId,
            createdAt: customer.CreatedAt,
            lastUpdatedAt: customer.LastUpdatedAt,
            externalRegisters: customer.ExternalRegisters.ToList());

        _customerRepository.GetByApiIdAsync("API123", Arg.Any<CancellationToken>())
            .Returns(reconstituted);

        var query = new GetCustomerQuery(ApiId: "API123");

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("API123", result.Value.ApiId);

        await _customerRepository.Received(1).GetByApiIdAsync("API123", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithNoIdentifier_ShouldReturnInvalidQueryError()
    {
        // Arrange
        var query = new GetCustomerQuery(Id: null, ApiId: null);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Customer.InvalidQuery", result.Error.Code);
        Assert.Contains("Either Id or ApiId must be provided", result.Error.Message);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentId_ShouldReturnNotFoundError()
    {
        // Arrange
        _customerRepository.GetByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((Customer?)null);

        var query = new GetCustomerQuery(Id: 999);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Customer.NotFound", result.Error.Code);
        Assert.Contains("999", result.Error.Message);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentApiId_ShouldReturnNotFoundError()
    {
        // Arrange
        _customerRepository.GetByApiIdAsync("NONEXISTENT", Arg.Any<CancellationToken>())
            .Returns((Customer?)null);

        var query = new GetCustomerQuery(ApiId: "NONEXISTENT");

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Customer.NotFound", result.Error.Code);
        Assert.Contains("NONEXISTENT", result.Error.Message);
    }

    [Fact]
    public async Task HandleAsync_ShouldPrioritizeIdOverApiId()
    {
        // Arrange
        var customer = Customer.Create(
            apiId: "API123",
            document: "12345678901",
            company: Company.Warren);

        var reconstituted = Customer.Reconstitute(
            id: 1,
            apiId: customer.ApiId,
            document: customer.Document.Value,
            sinacorId: customer.SinacorId,
            company: customer.Company,
            legacyExternalId: customer.LegacyExternalId,
            createdAt: customer.CreatedAt,
            lastUpdatedAt: customer.LastUpdatedAt,
            externalRegisters: customer.ExternalRegisters.ToList());

        _customerRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(reconstituted);

        var query = new GetCustomerQuery(Id: 1, ApiId: "API123");

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.True(result.IsSuccess);

        await _customerRepository.Received(1).GetByIdAsync(1, Arg.Any<CancellationToken>());
        await _customerRepository.DidNotReceive().GetByApiIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ShouldIncludeExternalRegisters()
    {
        // Arrange
        var customer = Customer.Create(
            apiId: "API123",
            document: "12345678901",
            company: Company.Warren);

        var reconstituted = Customer.Reconstitute(
            id: 1,
            apiId: customer.ApiId,
            document: customer.Document.Value,
            sinacorId: customer.SinacorId,
            company: customer.Company,
            legacyExternalId: customer.LegacyExternalId,
            createdAt: customer.CreatedAt,
            lastUpdatedAt: customer.LastUpdatedAt,
            externalRegisters: customer.ExternalRegisters.ToList());

        _customerRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(reconstituted);

        var query = new GetCustomerQuery(Id: 1);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.ExternalRegisters.Count);
        Assert.Contains(result.Value.ExternalRegisters, r => r.SystemType == "Cetip");
        Assert.Contains(result.Value.ExternalRegisters, r => r.SystemType == "Selic");
    }
}
