using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class TenantStatus : LookupEntity
{
    public string Code { get; set; } = null!;
    public string Description { get; set; } = "";

    // Navigation properties
    public ICollection<Tenant> Tenants { get; private set; } = [];

    private TenantStatus() { }

    public static TenantStatus Create(string code, string name, TranslatedString translations, string description = "")
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        var tenantStatus = new TenantStatus
        {
            Code = code.ToLower().Trim(),
            Name = name.Trim(),
            Description = description.Trim(),
            Translations = translations
        };

        tenantStatus.InitializeAudit();
        return tenantStatus;
    }

    public void UpdateDetails(string name, string description, TranslatedString translations)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        Name = name.Trim();
        Description = description.Trim();
        UpdateTranslations(translations);
    }
}
