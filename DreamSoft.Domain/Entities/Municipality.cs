using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class Municipality : LookupEntity
{
    public int ProvinceId { get; protected set; }
    public string? Code { get; protected set; }

    // Navigation properties
    public Province Province { get; private set; } = null!;
    public ICollection<Tenant> Tenants { get; private set; } = [];

    private Municipality() { }

    public static Municipality Create(int provinceId, string name, TranslatedString translations, string? code = null)
    {
        if (provinceId <= 0)
            throw new ArgumentException("Province ID is required", nameof(provinceId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations);

        var municipality = new Municipality
        {
            ProvinceId = provinceId,
            Name = name.Trim(),
            Code = code?.ToUpper().Trim(),
            Translations = translations
        };

        municipality.InitializeAudit();
        return municipality;
    }
}
