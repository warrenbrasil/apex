namespace Apex.Domain.Enums;

/// <summary>
/// Represents the registration status of a customer in an external system (CETIP or SELIC).
/// </summary>
public enum CustomerExternalSystemStatus : byte
{
    /// <summary>
    /// Customer is not yet registered in the external system.
    /// </summary>
    NotRegistered = 0,

    /// <summary>
    /// Customer is registered and active in the external system.
    /// </summary>
    Registered = 1,

    /// <summary>
    /// Customer registration is inactive in the external system.
    /// </summary>
    Inactive = 2
}
