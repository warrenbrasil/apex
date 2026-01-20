namespace Apex.Domain.Enums;

/// <summary>
/// Represents the type of entity that issues bonds.
/// </summary>
public enum EmitterType
{
    /// <summary>
    /// Financial institution - Banks, credit unions, etc.
    /// </summary>
    FinancialInstitution = 10,

    /// <summary>
    /// Company - Private corporations.
    /// </summary>
    Company = 20,

    /// <summary>
    /// Individual - Natural person.
    /// </summary>
    Individual = 30,

    /// <summary>
    /// Union - Federal government.
    /// </summary>
    Union = 40,

    /// <summary>
    /// State - State government.
    /// </summary>
    State = 50,

    /// <summary>
    /// City - Municipal government.
    /// </summary>
    City = 60
}
