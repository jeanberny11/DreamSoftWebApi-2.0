using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class Country : LookupEntity
{
    public string IsoCode2 { get; protected set; } = null!;
    public string IsoCode3 { get; protected set; } = null!;
    public string? PhoneCode { get; protected set; }

    // Navigation properties
    public ICollection<Province> Provinces { get; private set; } = [];
    public ICollection<Tenant> Tenants { get; private set; } = [];

    private Country() { }

    public static Country Create(string name, string isoCode2, string isoCode3, TranslatedString translations, string? phoneCode = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(isoCode2) || isoCode2.Length != 2)
            throw new ArgumentException("ISO Code 2 must be 2 characters", nameof(isoCode2));

        if (string.IsNullOrWhiteSpace(isoCode3) || isoCode3.Length != 3)
            throw new ArgumentException("ISO Code 3 must be 3 characters", nameof(isoCode3));

        ArgumentNullException.ThrowIfNull(translations);

        var country = new Country
        {
            Name = name.Trim(),
            IsoCode2 = isoCode2.ToUpper().Trim(),
            IsoCode3 = isoCode3.ToUpper().Trim(),
            PhoneCode = phoneCode?.Trim(),
            Translations = translations
        };

        country.InitializeAudit();
        return country;
    }
}
