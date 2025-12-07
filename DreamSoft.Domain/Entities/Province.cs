using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class Province : LookupEntity
{
    public int CountryId { get; protected set; }
    public string? Code { get; protected set; }

    // Navigation properties
    public Country Country { get; private set; } = null!;
    public ICollection<Municipality> Municipalities { get; private set; } = [];
    public ICollection<Tenant> Tenants { get; private set; } = [];

    private Province() { }

    public static Province Create(int countryId, string name, TranslatedString translations, string? code = null)
    {
        if (countryId <= 0)
            throw new ArgumentException("Country ID is required", nameof(countryId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations);

        var province = new Province
        {
            CountryId = countryId,
            Name = name.Trim(),
            Code = code?.ToUpper().Trim(),
            Translations = translations
        };

        province.InitializeAudit();
        return province;
    }
}
