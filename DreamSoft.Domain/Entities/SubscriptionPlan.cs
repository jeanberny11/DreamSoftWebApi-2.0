using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class SubscriptionPlan : AuditableEntity
{
    public int TierId { get; protected set; }
    public int BillingCycleId { get; protected set; }
    public string PlanName { get; protected set; } = null!;
    public string? Description { get; protected set; }
    public TranslatedString Translations { get; protected set; } = null!; // FIXED: Now required (NOT NULL in DB)
    public decimal Price { get; protected set; }
    public string? StripePriceId { get; protected set; }
    public int? TrialDays { get; protected set; }

    // Resource limits (moved from SubscriptionTier)
    public int MaxUsers { get; protected set; }
    public int MaxStorageGb { get; protected set; }
    public int? MaxInvoicesPerMonth { get; protected set; }

    // Navigation properties
    public SubscriptionTier Tier { get; private set; } = null!;
    public BillingCycle BillingCycle { get; private set; } = null!;
    public ICollection<TenantSubscription> TenantSubscriptions { get; private set; } = [];

    private SubscriptionPlan() { }

    public static SubscriptionPlan Create(
        int tierId, 
        int billingCycleId,
        string planName,
        TranslatedString translations,
        decimal price,
        int maxUsers,
        int maxStorageGb,
        int? maxInvoicesPerMonth = null,
        string? description = null,
        string? stripePriceId = null,
        int? trialDays = null)
    {
        if (tierId <= 0)
            throw new ArgumentException("Tier ID is required", nameof(tierId));

        if (billingCycleId <= 0)
            throw new ArgumentException("Billing cycle ID is required", nameof(billingCycleId));

        if (string.IsNullOrWhiteSpace(planName))
            throw new ArgumentException("Plan name is required", nameof(planName));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        if (price < 0)
            throw new ArgumentException("Price must be non-negative", nameof(price));

        if (maxUsers <= 0)
            throw new ArgumentException("Max users must be greater than zero", nameof(maxUsers));

        if (maxStorageGb <= 0)
            throw new ArgumentException("Max storage must be greater than zero", nameof(maxStorageGb));

        if (trialDays.HasValue && trialDays.Value < 0)
            throw new ArgumentException("Trial days must be non-negative", nameof(trialDays));

        var plan = new SubscriptionPlan
        {
            TierId = tierId,
            BillingCycleId = billingCycleId,
            PlanName = planName.Trim(),
            Description = description?.Trim(),
            Translations = translations,
            Price = price,
            MaxUsers = maxUsers,
            MaxStorageGb = maxStorageGb,
            MaxInvoicesPerMonth = maxInvoicesPerMonth,
            StripePriceId = stripePriceId?.Trim(),
            TrialDays = trialDays
        };

        plan.InitializeAudit();
        return plan;
    }

    public void UpdatePrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentException("Price must be non-negative", nameof(price));

        Price = price;
        MarkAsUpdated();
    }

    public void UpdateLimits(int maxUsers, int maxStorageGb, int? maxInvoicesPerMonth = null)
    {
        if (maxUsers <= 0)
            throw new ArgumentException("Max users must be greater than zero", nameof(maxUsers));

        if (maxStorageGb <= 0)
            throw new ArgumentException("Max storage must be greater than zero", nameof(maxStorageGb));

        MaxUsers = maxUsers;
        MaxStorageGb = maxStorageGb;
        MaxInvoicesPerMonth = maxInvoicesPerMonth;
        MarkAsUpdated();
    }

    public void UpdateStripeIntegration(string stripePriceId)
    {
        if (string.IsNullOrWhiteSpace(stripePriceId))
            throw new ArgumentException("Stripe price ID is required", nameof(stripePriceId));

        StripePriceId = stripePriceId.Trim();
        MarkAsUpdated();
    }
}
