namespace Apex.Domain.Enums;

/// <summary>
/// Represents the market index used for bond rate calculation.
/// </summary>
public enum MarketIndexType
{
    /// <summary>
    /// Pre-fixado - Fixed rate bond (no index).
    /// </summary>
    Pre = 0,

    /// <summary>
    /// CDI - Certificado de Depósito Interbancário (Interbank Deposit Certificate rate).
    /// </summary>
    Cdi = 10,

    /// <summary>
    /// IPCA - Índice Nacional de Preços ao Consumidor Amplo (Inflation index).
    /// </summary>
    Ipca = 20,

    /// <summary>
    /// Poupança - Savings rate.
    /// </summary>
    Savings = 30,

    /// <summary>
    /// SELIC - Sistema Especial de Liquidação e Custódia (Central Bank rate).
    /// </summary>
    Selic = 40,

    /// <summary>
    /// IGP-M - Índice Geral de Preços do Mercado (General Market Price Index).
    /// </summary>
    IgpM = 50,

    /// <summary>
    /// IBOVESPA - Índice da Bolsa de Valores de São Paulo.
    /// </summary>
    Ibovespa = 60,

    /// <summary>
    /// S&P 500 - Standard & Poor's 500 Index.
    /// </summary>
    Sp500 = 70,

    /// <summary>
    /// No index - Used for special cases.
    /// </summary>
    NoIndex = 200
}
