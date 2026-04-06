using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class IdType : LookupEntity
{
    public string Code { get; set; } = null!;
    public int CountryId { get; set; }
    public string ValidationPattern { get; set; } = null!;

    // Navigation properties
    public Country Country { get; set; } = null!;

    private IdType() { }

    public static IdType Create(string code, string name, int countryId, string validationPattern, TranslatedString translations)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        if (countryId <= 0)
            throw new ArgumentException("Country ID must be greater than zero", nameof(countryId));

        if (string.IsNullOrWhiteSpace(validationPattern))
            throw new ArgumentException("Validation pattern is required", nameof(validationPattern));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        var idType = new IdType
        {
            Code = code.ToUpper().Trim(),
            Name = name.Trim(),
            CountryId = countryId,
            ValidationPattern = validationPattern.Trim(),
            Translations = translations
        };

        idType.InitializeAudit();
        return idType;
    }

    public void UpdateValidationPattern(string validationPattern)
    {
        if (string.IsNullOrWhiteSpace(validationPattern))
            throw new ArgumentException("Validation pattern is required", nameof(validationPattern));

        ValidationPattern = validationPattern.Trim();
        MarkAsUpdated();
    }
}
