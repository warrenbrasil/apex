using Apex.Application.Abstractions.Messaging;
using Apex.Application.Common;
using Apex.Application.Customers.DTOs;
using Apex.Domain.Entities;
using Apex.Domain.Repositories;

namespace Apex.Application.Customers.Queries.GetCustomer;

/// <summary>
/// Handler for GetCustomerQuery.
/// </summary>
public sealed class GetCustomerQueryHandler
    : IQueryHandler<GetCustomerQuery, Result<CustomerResponse>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<CustomerResponse>> HandleAsync(
        GetCustomerQuery query,
        CancellationToken cancellationToken = default)
    {
        // Validate that at least one identifier is provided
        if (!query.Id.HasValue && string.IsNullOrWhiteSpace(query.ApiId))
        {
            return Result.Failure<CustomerResponse>(
                new Error(
                    "Customer.InvalidQuery",
                    "Either Id or ApiId must be provided."));
        }

        // Get customer by ID or API ID
        Customer? customer = null;

        if (query.Id.HasValue)
        {
            customer = await _customerRepository.GetByIdAsync(query.Id.Value, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(query.ApiId))
        {
            customer = await _customerRepository.GetByApiIdAsync(query.ApiId, cancellationToken);
        }

        // Check if customer was found
        if (customer is null)
        {
            var identifier = query.Id?.ToString() ?? query.ApiId ?? "unknown";
            return Result.Failure<CustomerResponse>(
                new Error(
                    "Customer.NotFound",
                    $"Customer not found with identifier: {identifier}"));
        }

        // Map to response
        var response = MapToResponse(customer);

        return Result.Success(response);
    }

    private static CustomerResponse MapToResponse(Customer customer)
    {
        return new CustomerResponse
        {
            Id = customer.Id,
            ApiId = customer.ApiId,
            Document = customer.Document.Value,
            SinacorId = customer.SinacorId,
            Company = customer.Company.ToString(),
            LegacyExternalId = customer.LegacyExternalId,
            CreatedAt = customer.CreatedAt,
            LastUpdatedAt = customer.LastUpdatedAt,
            ExternalRegisters = customer.ExternalRegisters.Select(r => new ExternalSystemRegisterResponse
            {
                Id = r.Id,
                Status = r.Status.ToString(),
                SystemType = r.SystemType.ToString(),
                CreatedAt = r.CreatedAt,
                LastUpdatedAt = r.LastUpdatedAt
            }).ToList()
        };
    }
}
