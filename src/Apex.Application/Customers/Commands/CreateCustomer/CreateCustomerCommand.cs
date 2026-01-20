using Apex.Application.Abstractions.Messaging;
using Apex.Application.Common;
using Apex.Application.Customers.DTOs;

namespace Apex.Application.Customers.Commands.CreateCustomer;

/// <summary>
/// Command to create a new customer.
/// </summary>
public sealed record CreateCustomerCommand(
    string ApiId,
    string Document,
    int Company,
    string? SinacorId = null,
    string? LegacyExternalId = null) : ICommand<Result<CustomerResponse>>;
