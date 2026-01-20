namespace Apex.Domain.Enums;

/// <summary>
/// Represents the custody chamber where bonds are held.
/// </summary>
public enum CustodyChamberType
{
    /// <summary>
    /// CETIP - Central de Custódia e Liquidação Financeira de Títulos Privados.
    /// Used for corporate bonds, CRIs, CRAs, debentures, etc.
    /// </summary>
    Cetip = 1,

    /// <summary>
    /// SELIC - Sistema Especial de Liquidação e Custódia.
    /// Used for government securities (Tesouro Direto).
    /// </summary>
    Selic = 2
}
