using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class TenantSubscription : AuditableEntity
{
    public int TenantId { get; protected set; }
    public int PlanId { get; protected set; }
    public string Status { get; protected set; } = null!;
    public DateTime CurrentPeriodStart { get; protected set; }
    public DateTime CurrentPeriodEnd { get; protected set; }
    public string? StripeCustomerId { get; protected set; }
    public string? StripeSubscriptionId { get; protected set; }

    // Navigation properties
    public Tenant Tenant { get; private set; } = null!;
    public SubscriptionPlan Plan { get; private set; } = null!;
    public ICollection<Payment> Payments { get; private set; } = [];

    private TenantSubscription() { }

    public static TenantSubscription Create(int tenantId, int planId, string status, DateTime currentPeriodStart, DateTime currentPeriodEnd, string? stripeCustomerId = null, string? stripeSubscriptionId = null)
    {
        if (tenantId <= 0)
            throw new ArgumentException("Tenant ID is required", nameof(tenantId));

        if (planId <= 0)
            throw new ArgumentException("Plan ID is required", nameof(planId));

        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status is required", nameof(status));

        var subscription = new TenantSubscription
        {
            TenantId = tenantId,
            PlanId = planId,
            Status = status.ToLower().Trim(),
            CurrentPeriodStart = currentPeriodStart,
            CurrentPeriodEnd = currentPeriodEnd,
            StripeCustomerId = stripeCustomerId?.Trim(),
            StripeSubscriptionId = stripeSubscriptionId?.Trim()
        };

        subscription.InitializeAudit();
        return subscription;
    }
}
