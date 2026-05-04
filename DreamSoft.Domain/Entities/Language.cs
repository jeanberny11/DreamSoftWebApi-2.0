using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class Language : LookupEntity
{
    public string Code { get; set; } = null!;
    public bool IsDefault { get; set; }

    // Navigation properties
    public ICollection<Tenant> Tenants { get; private set; } = [];
    public ICollection<User> Users { get; private set; } = [];

    private Language() { }

    public static Language Create(string code, string name, TranslatedString translations, bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        var language = new Language
        {
            Code = code.ToLower().Trim(),
            Name = name.Trim(),
            Translations = translations,
            IsDefault = isDefault
        };

        language.InitializeAudit();
        return language;
    }

    public void UpdateDetails(string name, TranslatedString translations, bool isDefault, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        Name = name.Trim();
        IsDefault = isDefault;
        UpdateTranslations(translations);

        if (isActive && !IsActive)
            Activate();
        else if (!isActive && IsActive)
            Deactivate();

        MarkAsUpdated();
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
