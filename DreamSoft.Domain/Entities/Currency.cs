using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class Currency : AuditableEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string NativeName { get; set; } = null!;
    public bool IsDefault { get; set; }

    // Navigation properties
    public ICollection<Tenant> Tenants { get; private set; } = [];

    private Currency() { }

    public static Currency Create(string code, string name, string nativeName, bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(nativeName))
            throw new ArgumentException("Native name is required", nameof(nativeName));

        var currency = new Currency
        {
            Code = code.ToUpper().Trim(),
            Name = name.Trim(),
            NativeName = nativeName.Trim(),
            IsDefault = isDefault
        };

        currency.InitializeAudit();
        return currency;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        MarkAsUpdated();
    }

    public void RemoveDefault()
    {
        IsDefault = false;
        MarkAsUpdated();
    }
}
