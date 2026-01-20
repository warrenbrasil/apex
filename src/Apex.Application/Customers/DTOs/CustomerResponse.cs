namespace Apex.Application.Customers.DTOs;

/// <summary>
/// Response DTO for Customer operations.
/// </summary>
public sealed class CustomerResponse
{
    public int Id { get; init; }
    public string ApiId { get; init; } = string.Empty;
    public string Document { get; init; } = string.Empty;
    public string? SinacorId { get; init; }
    public string Company { get; init; } = string.Empty;
    public string? LegacyExternalId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastUpdatedAt { get; init; }
    public List<ExternalSystemRegisterResponse> ExternalRegisters { get; init; } = new();
}

/// <summary>
/// Response DTO for external system register.
/// </summary>
public sealed class ExternalSystemRegisterResponse
{
    public int Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public string SystemType { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? LastUpdatedAt { get; init; }
}
