using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class SubscriptionPlan : AuditableEntity
{
    public int TierId { get; protected set; }
    public string PlanName { get; protected set; } = null!;
    public string? Description { get; protected set; }
    public TranslatedString? Translations { get; protected set; }
    public int BillingCycleId { get; protected set; }
    public decimal PriceMonthly { get; protected set; }
    public decimal? PriceYearly { get; protected set; }
    public int? MaxUsers { get; protected set; }
    public int? MaxStorageGb { get; protected set; }
    public int? MaxInvoicesPerMonth { get; protected set; }

    // Navigation properties
    public SubscriptionTier Tier { get; private set; } = null!;
    public BillingCycle BillingCycle { get; private set; } = null!;
    public ICollection<TenantSubscription> TenantSubscriptions { get; private set; } = [];

    private SubscriptionPlan() { }

    public static SubscriptionPlan Create(int tierId, string planName, int billingCycleId, decimal priceMonthly, string? description = null, decimal? priceYearly = null, int? maxUsers = null, int? maxStorageGb = null, int? maxInvoicesPerMonth = null, TranslatedString? translations = null)
    {
        if (tierId <= 0)
            throw new ArgumentException("Tier ID is required", nameof(tierId));

        if (string.IsNullOrWhiteSpace(planName))
            throw new ArgumentException("Plan name is required", nameof(planName));

        if (billingCycleId <= 0)
            throw new ArgumentException("Billing cycle ID is required", nameof(billingCycleId));

        if (priceMonthly < 0)
            throw new ArgumentException("Price monthly must be non-negative", nameof(priceMonthly));

        var plan = new SubscriptionPlan
        {
            TierId = tierId,
            PlanName = planName.Trim(),
            Description = description?.Trim(),
            BillingCycleId = billingCycleId,
            PriceMonthly = priceMonthly,
            PriceYearly = priceYearly,
            MaxUsers = maxUsers,
            MaxStorageGb = maxStorageGb,
            MaxInvoicesPerMonth = maxInvoicesPerMonth,
            Translations = translations
        };

        plan.InitializeAudit();
        return plan;
    }
}
