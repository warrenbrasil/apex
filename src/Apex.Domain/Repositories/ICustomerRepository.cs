using Apex.Domain.Entities;

namespace Apex.Domain.Repositories;

/// <summary>
/// Repository interface for Customer aggregate operations.
/// </summary>
public interface ICustomerRepository
{
    /// <summary>
    /// Adds a new customer to the repository.
    /// </summary>
    /// <param name="customer">The customer to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a customer by ID.
    /// </summary>
    /// <param name="id">The customer ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer if found, null otherwise.</returns>
    Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a customer by API ID.
    /// </summary>
    /// <param name="apiId">The customer API ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer if found, null otherwise.</returns>
    Task<Customer?> GetByApiIdAsync(string apiId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    /// <param name="customer">The customer to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a customer exists with the specified document, Sinacor ID, and company.
    /// </summary>
    /// <param name="document">The business document (CPF/CNPJ).</param>
    /// <param name="sinacorId">The Sinacor ID.</param>
    /// <param name="company">The company.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if a customer exists, false otherwise.</returns>
    Task<bool> ExistsAsync(string document, string? sinacorId, int company, CancellationToken cancellationToken = default);
}
