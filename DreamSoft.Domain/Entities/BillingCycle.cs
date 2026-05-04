using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class BillingCycle : LookupEntity
{
    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int Months { get; protected set; }

    // Navigation properties
    public ICollection<PlanPrice> PlanPrices { get; private set; } = [];

    private BillingCycle() { }

    public static BillingCycle Create(string code, string name, string description, TranslatedString translations, int months)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        if (months <= 0)
            throw new ArgumentException("Months must be greater than zero", nameof(months));

        var billingCycle = new BillingCycle
        {
            Code = code.Trim(),
            Description = description.Trim(),
            Name = name.Trim(),
            Translations = translations,
            Months = months
        };

        billingCycle.InitializeAudit();
        return billingCycle;
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

    public void SetActive(bool isActive)
    {
        if (isActive) Activate(); else Deactivate();
    }
}
