using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class Municipality : AuditableEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int ProvinceId { get; set; }

    // Navigation properties
    public Province Province { get; set; } = null!;
    public ICollection<Tenant> Tenants { get; private set; } = [];

    private Municipality() { }

    public static Municipality Create(string code, string name, int provinceId)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        if (provinceId <= 0)
            throw new ArgumentException("Province ID must be greater than zero", nameof(provinceId));

        var municipality = new Municipality
        {
            Code = code.ToUpper().Trim(),
            Name = name.Trim(),
            ProvinceId = provinceId,
        };

        municipality.InitializeAudit();
        return municipality;
    }
}
