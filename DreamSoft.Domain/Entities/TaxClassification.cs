using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class TaxClassification : LookupEntity
{
    public string Code { get; protected set; } = null!;
    public string? Description { get; protected set; }
    public string? NcfType { get; protected set; }
    public bool RequiresRnc { get; protected set; }

    // Navigation properties
    public ICollection<Customer> Customers { get; private set; } = [];

    private TaxClassification() { }

    public static TaxClassification Create(string code, string name, TranslatedString? translations = null, string? description = null, string? ncfType = null, bool requiresRnc = false)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        var taxClassification = new TaxClassification
        {
            Code = code.ToUpper().Trim(),
            Name = name.Trim(),
            Description = description?.Trim(),
            NcfType = ncfType?.Trim(),
            RequiresRnc = requiresRnc,
            Translations = translations
        };

        taxClassification.InitializeAudit();
        return taxClassification;
    }
}
