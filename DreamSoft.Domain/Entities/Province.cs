using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class Province : LookupEntity
{
    public string Code { get; set; } = null!;
    public int CountryId { get; set; }

    // Navigation properties
    public Country Country { get; set; } = null!;
    public ICollection<Municipality> Municipalities { get; private set; } = [];
    public ICollection<Tenant> Tenants { get; private set; } = [];

    private Province() { }

    public static Province Create(string code, string name, int countryId)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        if (countryId <= 0)
            throw new ArgumentException("Country ID must be greater than zero", nameof(countryId));

        var province = new Province
        {
            Code = code.ToUpper().Trim(),
            Name = name.Trim(),
            CountryId = countryId,
            Translations = null! // Provinces may not have translations in the DB schema
        };

        province.InitializeAudit();
        return province;
    }
}
