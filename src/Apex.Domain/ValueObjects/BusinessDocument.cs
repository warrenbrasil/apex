using Apex.Domain.Primitives;

namespace Apex.Domain.ValueObjects;

/// <summary>
/// Value object representing a Brazilian business document (CNPJ or CPF).
/// </summary>
public sealed class BusinessDocument : ValueObject
{
    private const int CnpjLength = 14;
    private const int CpfLength = 11;

    private BusinessDocument(string value, BusinessDocumentType type)
    {
        Value = value;
        Type = type;
    }

    public string Value { get; }
    public BusinessDocumentType Type { get; }

    /// <summary>
    /// Gets whether this is a CNPJ (company document).
    /// </summary>
    public bool IsCnpj => Type == BusinessDocumentType.Cnpj;

    /// <summary>
    /// Gets whether this is a CPF (individual document).
    /// </summary>
    public bool IsCpf => Type == BusinessDocumentType.Cpf;

    /// <summary>
    /// Creates a BusinessDocument from a string value.
    /// Automatically detects if it's CPF or CNPJ based on length.
    /// </summary>
    /// <param name="value">The document number (digits only).</param>
    /// <returns>A new BusinessDocument instance.</returns>
    public static BusinessDocument Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Business document cannot be null or empty.", nameof(value));
        }

        // Remove non-digit characters
        var digitsOnly = new string(value.Where(char.IsDigit).ToArray());

        if (digitsOnly.Length != CpfLength && digitsOnly.Length != CnpjLength)
        {
            throw new ArgumentException($"Business document must have {CpfLength} (CPF) or {CnpjLength} (CNPJ) digits.", nameof(value));
        }

        var type = digitsOnly.Length == CnpjLength ? BusinessDocumentType.Cnpj : BusinessDocumentType.Cpf;

        return new BusinessDocument(digitsOnly, type);
    }

    /// <summary>
    /// Formats the document with standard mask.
    /// CPF: 000.000.000-00
    /// CNPJ: 00.000.000/0000-00
    /// </summary>
    public string FormatWithMask()
    {
        return Type switch
        {
            BusinessDocumentType.Cpf => $"{Value.Substring(0, 3)}.{Value.Substring(3, 3)}.{Value.Substring(6, 3)}-{Value.Substring(9, 2)}",
            BusinessDocumentType.Cnpj => $"{Value.Substring(0, 2)}.{Value.Substring(2, 3)}.{Value.Substring(5, 3)}/{Value.Substring(8, 4)}-{Value.Substring(12, 2)}",
            _ => Value
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
        yield return Type;
    }

    public override string ToString() => Value;

    public static implicit operator string(BusinessDocument document) => document.Value;
}

/// <summary>
/// Type of business document.
/// </summary>
public enum BusinessDocumentType
{
    /// <summary>
    /// CPF - Cadastro de Pessoas Físicas (Individual Taxpayer Registry).
    /// </summary>
    Cpf,

    /// <summary>
    /// CNPJ - Cadastro Nacional da Pessoa Jurídica (National Registry of Legal Entities).
    /// </summary>
    Cnpj
}
