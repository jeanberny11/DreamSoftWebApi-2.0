using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class SubscriptionPlan : LookupEntity
{
    public string Code { get; set; } = null!;
    public string Description { get; set; } = "";
    public int? SolutionId { get; set; }
    public int BillingCycleId { get; set; }
    public decimal Price { get; set; } = 0;
    public int TrialDays { get; set; }

    // Navigation properties
    public Solution Solution { get; set; } = null!;
    public BillingCycle BillingCycle { get; set; } = null!;
    public ICollection<TenantSubscription> TenantSubscriptions { get; private set; } = [];

    private SubscriptionPlan() { }

    public static SubscriptionPlan Create(
        string code,
        string name,
        TranslatedString translations,
        int billingCycleId,
        string description = "",
        int? solutionId = null,
        decimal price = 0,
        int trialDays = 0)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        if (billingCycleId <= 0)
            throw new ArgumentException("Billing cycle ID must be greater than zero", nameof(billingCycleId));

        if ( price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        if (trialDays < 0)
            throw new ArgumentException("Trial days cannot be negative", nameof(trialDays));

        var plan = new SubscriptionPlan
        {
            Code = code.ToUpper().Trim(),
            Name = name.Trim(),
            Description = description.Trim(),
            Translations = translations,
            SolutionId = solutionId,
            BillingCycleId = billingCycleId,
            Price = price,
            TrialDays = trialDays
        };

        plan.InitializeAudit();
        return plan;
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

    public void UpdatePrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        Price = price;
        MarkAsUpdated();
    }

    public void UpdateSolution(int? solutionId)
    {
        SolutionId = solutionId;
        MarkAsUpdated();
    }

    public void UpdateTrialDays(int trialDays)
    {
        if (trialDays < 0)
            throw new ArgumentException("Trial days cannot be negative", nameof(trialDays));

        TrialDays = trialDays;
        MarkAsUpdated();
    }

    public bool HasTrial() => TrialDays > 0;
}
