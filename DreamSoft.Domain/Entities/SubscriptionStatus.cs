using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class SubscriptionStatus : LookupEntity
{
    public string Code { get; set; } = null!;

    // Navigation properties
    public ICollection<TenantSubscription> TenantSubscriptions { get; private set; } = [];

    private SubscriptionStatus() { }

    public static SubscriptionStatus Create(
        string code,
        string name,
        TranslatedString translations)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        var status = new SubscriptionStatus
        {
            Code = code.ToUpper().Trim(),
            Name = name.Trim(),
            Translations = translations
        };

        status.InitializeAudit();
        return status;
    }

    public void UpdateDetails(string name, TranslatedString translations)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        Name = name.Trim();
        UpdateTranslations(translations);
    }
}
