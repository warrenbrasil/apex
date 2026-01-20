using Apex.Domain.Enums;
using Apex.Domain.Primitives;

namespace Apex.Domain.Entities;

/// <summary>
/// Represents a customer's registration status in an external settlement system (CETIP or SELIC).
/// This is a child entity owned by Customer aggregate.
/// </summary>
public sealed class CustomerExternalSystemRegister : Entity<int>, IAuditable
{
    private CustomerExternalSystemRegister(
        int id,
        CustomerExternalSystemStatus status,
        CustomerExternalSystemType systemType,
        int customerId,
        DateTime createdAt)
        : base(id)
    {
        Status = status;
        SystemType = systemType;
        CustomerId = customerId;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Gets the registration status in the external system.
    /// </summary>
    public CustomerExternalSystemStatus Status { get; private set; }

    /// <summary>
    /// Gets the type of external system (CETIP or SELIC).
    /// </summary>
    public CustomerExternalSystemType SystemType { get; private init; }

    /// <summary>
    /// Gets the foreign key to Customer.
    /// </summary>
    public int CustomerId { get; private init; }

    /// <summary>
    /// Gets the date and time when the register was created.
    /// </summary>
    public DateTime CreatedAt { get; private init; }

    /// <summary>
    /// Gets the date and time when the register was last updated.
    /// </summary>
    public DateTime? LastUpdatedAt { get; private set; }

    /// <summary>
    /// Gets whether the customer is registered in the external system.
    /// </summary>
    public bool IsRegistered => Status == CustomerExternalSystemStatus.Registered;

    /// <summary>
    /// Gets whether the customer registration is active.
    /// </summary>
    public bool IsActive => Status == CustomerExternalSystemStatus.Registered;

    /// <summary>
    /// Gets whether the customer is not registered.
    /// </summary>
    public bool IsNotRegistered => Status == CustomerExternalSystemStatus.NotRegistered;

    /// <summary>
    /// Gets whether the customer registration is inactive.
    /// </summary>
    public bool IsInactive => Status == CustomerExternalSystemStatus.Inactive;

    /// <summary>
    /// Creates a new CustomerExternalSystemRegister (for new registers not yet persisted).
    /// </summary>
    /// <param name="systemType">The external system type.</param>
    /// <param name="customerId">The customer ID.</param>
    /// <param name="status">Initial status (default: NotRegistered).</param>
    /// <returns>A new CustomerExternalSystemRegister instance.</returns>
    public static CustomerExternalSystemRegister Create(
        CustomerExternalSystemType systemType,
        int customerId,
        CustomerExternalSystemStatus status = CustomerExternalSystemStatus.NotRegistered)
    {
        // CustomerId can be 0 during creation, will be set by repository
        return new CustomerExternalSystemRegister(
            id: 0, // Will be set by database
            status: status,
            systemType: systemType,
            customerId: customerId,
            createdAt: DateTime.UtcNow);
    }

    /// <summary>
    /// Reconstitutes a CustomerExternalSystemRegister from persistence (used by repository).
    /// </summary>
    public static CustomerExternalSystemRegister Reconstitute(
        int id,
        CustomerExternalSystemStatus status,
        CustomerExternalSystemType systemType,
        int customerId,
        DateTime createdAt,
        DateTime? lastUpdatedAt)
    {
        var register = new CustomerExternalSystemRegister(
            id: id,
            status: status,
            systemType: systemType,
            customerId: customerId,
            createdAt: createdAt)
        {
            LastUpdatedAt = lastUpdatedAt
        };

        return register;
    }

    /// <summary>
    /// Updates the registration status to Registered.
    /// </summary>
    public void MarkAsRegistered()
    {
        Status = CustomerExternalSystemStatus.Registered;
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the registration status to Inactive.
    /// </summary>
    public void MarkAsInactive()
    {
        Status = CustomerExternalSystemStatus.Inactive;
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the registration status to NotRegistered.
    /// </summary>
    public void MarkAsNotRegistered()
    {
        Status = CustomerExternalSystemStatus.NotRegistered;
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the registration status.
    /// </summary>
    public void UpdateStatus(CustomerExternalSystemStatus newStatus)
    {
        Status = newStatus;
        LastUpdatedAt = DateTime.UtcNow;
    }
}
