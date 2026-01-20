namespace Apex.Domain.Enums;

/// <summary>
/// Represents the type of external settlement/custody system.
/// </summary>
public enum CustomerExternalSystemType
{
    /// <summary>
    /// CETIP - Central de Custódia e de Liquidação Financeira de Títulos Privados.
    /// Used for private bond custody and settlement.
    /// </summary>
    Cetip = 0,

    /// <summary>
    /// SELIC - Sistema Especial de Liquidação e de Custódia.
    /// Used for government securities custody and settlement.
    /// </summary>
    Selic = 1
}
