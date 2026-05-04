using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class PlanPrice : AuditableEntity
{
    public int PlanId { get; private set; }
    public int BillingCycleId { get; private set; }
    public decimal Price { get; private set; }
    public string? StripePriceId { get; private set; }

    // Navigation properties
    public SubscriptionPlan Plan { get; private set; } = null!;
    public BillingCycle BillingCycle { get; private set; } = null!;
    public ICollection<TenantSubscription> TenantSubscriptions { get; private set; } = [];

    private PlanPrice() { }

    public static PlanPrice Create(int planId, int billingCycleId, decimal price, bool isActive = true)
    {
        if (planId <= 0)
            throw new ArgumentException("Plan ID must be greater than zero", nameof(planId));

        if (billingCycleId <= 0)
            throw new ArgumentException("Billing cycle ID must be greater than zero", nameof(billingCycleId));

        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        var planPrice = new PlanPrice
        {
            PlanId = planId,
            BillingCycleId = billingCycleId,
            Price = price,
            IsActive = isActive
        };

        planPrice.InitializeAudit();
        return planPrice;
    }

    public void UpdatePrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        Price = price;
        MarkAsUpdated();
    }

    public void SetActive(bool isActive) { IsActive = isActive; MarkAsUpdated(); }

    public void SetStripePriceId(string stripePriceId)
    {
        if (string.IsNullOrWhiteSpace(stripePriceId))
            throw new ArgumentException("Stripe price ID is required", nameof(stripePriceId));

        StripePriceId = stripePriceId.Trim();
        MarkAsUpdated();
    }
}
