namespace Apex.Api.Models;

/// <summary>
/// Request model for creating a new customer.
/// </summary>
public sealed class CreateCustomerRequest
{
    /// <summary>
    /// API identifier (max 32 characters).
    /// </summary>
    public required string ApiId { get; init; }

    /// <summary>
    /// CPF (11 digits) or CNPJ (14 digits).
    /// </summary>
    public required string Document { get; init; }

    /// <summary>
    /// Company: 1 = Warren, 2 = Rena.
    /// </summary>
    public required int Company { get; init; }

    /// <summary>
    /// Sinacor identifier (optional, max 9 characters).
    /// </summary>
    public string? SinacorId { get; init; }

    /// <summary>
    /// Legacy external identifier for migration (optional, max 9 characters).
    /// </summary>
    public string? LegacyExternalId { get; init; }
}
