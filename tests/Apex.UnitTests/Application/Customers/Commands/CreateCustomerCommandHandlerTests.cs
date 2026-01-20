using Apex.Application.Customers.Commands.CreateCustomer;
using Apex.Domain.Entities;
using Apex.Domain.Enums;
using Apex.Domain.Repositories;
using NSubstitute;

namespace Apex.UnitTests.Application.Customers.Commands;

public class CreateCustomerCommandHandlerTests
{
    private readonly ICustomerRepository _customerRepository;
    private readonly CreateCustomerCommandHandler _handler;

    public CreateCustomerCommandHandlerTests()
    {
        _customerRepository = Substitute.For<ICustomerRepository>();
        _handler = new CreateCustomerCommandHandler(_customerRepository);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateCustomerAndReturnSuccess()
    {
        // Arrange
        var command = new CreateCustomerCommand(
            ApiId: "API123",
            Document: "12345678901",
            Company: (int)Company.Warren,
            SinacorId: "123456789");

        _customerRepository.ExistsAsync(
            command.Document,
            command.SinacorId,
            command.Company,
            Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("API123", result.Value.ApiId);
        Assert.Equal("12345678901", result.Value.Document);
        Assert.Equal("Warren", result.Value.Company);
        Assert.Equal("123456789", result.Value.SinacorId);
        Assert.Equal(2, result.Value.ExternalRegisters.Count);

        await _customerRepository.Received(1).AddAsync(
            Arg.Any<Customer>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithExistingCustomer_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateCustomerCommand(
            ApiId: "API123",
            Document: "12345678901",
            Company: (int)Company.Warren,
            SinacorId: "123456789");

        _customerRepository.ExistsAsync(
            command.Document,
            command.SinacorId,
            command.Company,
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Customer.AlreadyExists", result.Error.Code);

        await _customerRepository.DidNotReceive().AddAsync(
            Arg.Any<Customer>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithInvalidApiId_ShouldReturnValidationError()
    {
        // Arrange
        var command = new CreateCustomerCommand(
            ApiId: "",
            Document: "12345678901",
            Company: (int)Company.Warren);

        _customerRepository.ExistsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Customer.ValidationFailed", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidDocument_ShouldReturnValidationError()
    {
        // Arrange
        var command = new CreateCustomerCommand(
            ApiId: "API123",
            Document: "123", // Invalid document
            Company: (int)Company.Warren);

        _customerRepository.ExistsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Customer.ValidationFailed", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_WithCnpj_ShouldCreateLegalEntityCustomer()
    {
        // Arrange
        var command = new CreateCustomerCommand(
            ApiId: "API123",
            Document: "12345678000190", // CNPJ
            Company: (int)Company.Rena);

        _customerRepository.ExistsAsync(
            command.Document,
            command.SinacorId,
            command.Company,
            Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("12345678000190", result.Value.Document);
        Assert.Equal("Rena", result.Value.Company);
    }

    [Fact]
    public async Task HandleAsync_WithLegacyExternalId_ShouldSetLegacyId()
    {
        // Arrange
        var command = new CreateCustomerCommand(
            ApiId: "API123",
            Document: "12345678901",
            Company: (int)Company.Warren,
            SinacorId: "123456789",
            LegacyExternalId: "LEGACY001");

        _customerRepository.ExistsAsync(
            command.Document,
            command.SinacorId,
            command.Company,
            Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("LEGACY001", result.Value.LegacyExternalId);
    }

    [Fact]
    public async Task HandleAsync_ShouldInitializeExternalRegisters()
    {
        // Arrange
        var command = new CreateCustomerCommand(
            ApiId: "API123",
            Document: "12345678901",
            Company: (int)Company.Warren);

        _customerRepository.ExistsAsync(
            command.Document,
            command.SinacorId,
            command.Company,
            Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.ExternalRegisters.Count);
        Assert.Contains(result.Value.ExternalRegisters, r => r.SystemType == "Cetip");
        Assert.Contains(result.Value.ExternalRegisters, r => r.SystemType == "Selic");
        Assert.All(result.Value.ExternalRegisters, r => Assert.Equal("NotRegistered", r.Status));
    }
}
