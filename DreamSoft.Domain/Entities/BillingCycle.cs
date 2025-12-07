using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class BillingCycle : LookupEntity
{
    public int Months { get; protected set; }

    // Navigation properties
    public ICollection<SubscriptionPlan> SubscriptionPlans { get; private set; } = [];

    private BillingCycle() { }

    public static BillingCycle Create(string name, TranslatedString translations, int months)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations);

        if (months <= 0)
            throw new ArgumentException("Months must be greater than zero", nameof(months));

        var billingCycle = new BillingCycle
        {
            Name = name.Trim(),
            Translations = translations,
            Months = months
        };

        billingCycle.InitializeAudit();
        return billingCycle;
    }
}
