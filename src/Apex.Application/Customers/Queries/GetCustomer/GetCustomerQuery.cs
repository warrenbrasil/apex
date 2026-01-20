using Apex.Application.Abstractions.Messaging;
using Apex.Application.Common;
using Apex.Application.Customers.DTOs;

namespace Apex.Application.Customers.Queries.GetCustomer;

/// <summary>
/// Query to get a customer by ID or API ID.
/// </summary>
public sealed record GetCustomerQuery(
    int? Id = null,
    string? ApiId = null) : IQuery<Result<CustomerResponse>>;
