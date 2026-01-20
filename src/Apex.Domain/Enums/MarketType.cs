namespace Apex.Domain.Enums;

/// <summary>
/// Represents the type of market where a bond is traded.
/// </summary>
public enum MarketType
{
    /// <summary>
    /// Primary market - New bond issuances directly from the issuer.
    /// </summary>
    Primary = 1,

    /// <summary>
    /// Secondary market - Trading of existing bonds between investors.
    /// </summary>
    Secondary = 2,

    /// <summary>
    /// IPO - Initial Public Offering.
    /// </summary>
    Ipo = 3
}
