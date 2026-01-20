using Apex.Application.Abstractions.Messaging;
using Apex.Application.Common;
using Apex.Application.Customers.DTOs;
using Apex.Domain.Entities;
using Apex.Domain.Enums;
using Apex.Domain.Exceptions;
using Apex.Domain.Primitives;
using Apex.Domain.Repositories;

namespace Apex.Application.Customers.Commands.CreateCustomer;

/// <summary>
/// Handler for CreateCustomerCommand.
/// </summary>
public sealed class CreateCustomerCommandHandler
    : ICommandHandler<CreateCustomerCommand, Result<CustomerResponse>>
{
    private readonly ICustomerRepository _customerRepository;

    public CreateCustomerCommandHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<CustomerResponse>> HandleAsync(
        CreateCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if customer already exists (unique constraint: Document + SinacorId + Company)
            if (await _customerRepository.ExistsAsync(command.Document, command.SinacorId, command.Company, cancellationToken))
            {
                return Result.Failure<CustomerResponse>(
                    new Error(
                        "Customer.AlreadyExists",
                        $"Customer with document '{command.Document}', Sinacor ID '{command.SinacorId}' and company '{command.Company}' already exists."));
            }

            // Create customer entity
            var customer = Customer.Create(
                apiId: command.ApiId,
                document: command.Document,
                company: (Company)command.Company,
                sinacorId: command.SinacorId,
                legacyExternalId: command.LegacyExternalId);

            // Persist customer
            await _customerRepository.AddAsync(customer, cancellationToken);

            // Map to response
            var response = MapToResponse(customer);

            return Result.Success(response);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<CustomerResponse>(
                new Error("Customer.ValidationFailed", ex.Message));
        }
        catch (DomainException ex)
        {
            return Result.Failure<CustomerResponse>(
                new Error("Customer.DomainError", ex.Message));
        }
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
