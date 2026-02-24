using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class Country : LookupEntity
{
    public string Code { get; set; } = null!;
    public string IsoCode { get; set; } = null!;
    public string PhoneCode { get; set; } = "";

    // Navigation properties
    public ICollection<Province> Provinces { get; private set; } = [];
    public ICollection<Tenant> Tenants { get; private set; } = [];
    public ICollection<IdType> IdTypes { get; private set; } = [];

    private Country() { }

    public static Country Create(string code, string name, string isoCode, TranslatedString translations, string? phoneCode = null)
    {
        ArgumentNullException.ThrowIfNull(translations);

        var country = new Country
        {
            Name = name.Trim(),
            Code = code.ToUpper().Trim(),
            IsoCode = isoCode.ToUpper().Trim(),
            PhoneCode = phoneCode ?? "",
            Translations = translations
        };

        country.InitializeAudit();
        return country;
    }
}
