using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

/// <summary>
/// Lookup table: Tax classification options (e.g. Final Consumer, Fiscal Credit).
/// Includes Dominican Republic-specific fields: NcfType and RequiresRnc.
/// Shared reference data across all tenants.
/// </summary>
public class TaxClassification : LookupEntity
{
    public string Code { get; private set; } = null!;

    /// <summary>
    /// Dominican Republic NCF (Número de Comprobante Fiscal) type associated
    /// with this tax classification (e.g. B01, B02, B14, B15).
    /// Optional — not all classifications require an NCF type.
    /// </summary>
    public string? NcfType { get; private set; }

    /// <summary>
    /// Indicates whether this tax classification requires a
    /// RNC (Registro Nacional del Contribuyente) number.
    /// Defaults to false.
    /// </summary>
    public bool RequiresRnc { get; private set; }

    // Navigation properties
    public ICollection<Customer> Customers { get; private set; } = [];

    private TaxClassification() { }

    public static TaxClassification Create(
        string code,
        string name,
        TranslatedString translations,
        string? ncfType = null,
        bool requiresRnc = false)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        var taxClassification = new TaxClassification
        {
            Code = code.ToUpper().Trim(),
            Name = name.Trim(),
            Translations = translations,
            NcfType = ncfType?.Trim(),
            RequiresRnc = requiresRnc
        };

        taxClassification.InitializeAudit();
        return taxClassification;
    }

    public void UpdateDetails(
        string name,
        TranslatedString translations,
        string? ncfType = null,
        bool requiresRnc = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        Name = name.Trim();
        NcfType = ncfType?.Trim();
        RequiresRnc = requiresRnc;
        UpdateTranslations(translations);
    }
}
