using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class TenantSubscription : AuditableEntity
{
    public int TenantId { get; private set; }
    public int SolutionId { get; private set; }
    public int SubscriptionPlanId { get; private set; }
    public int PlanPriceId { get; private set; }
    public int StatusId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime? TrialEndDate { get; private set; }
    public DateTime? CancellationScheduledAt { get; private set; }
    public string? StripeSubscriptionId { get; private set; }
    public string? StripeSessionId { get; private set; }
    public string? Notes { get; private set; }

    // Navigation properties
    public Tenant Tenant { get; private set; } = null!;
    public Solution Solution { get; private set; } = null!;
    public SubscriptionPlan SubscriptionPlan { get; private set; } = null!;
    public PlanPrice PlanPrice { get; private set; } = null!;
    public SubscriptionStatus Status { get; private set; } = null!;
    public ICollection<SubscriptionInvoice> Invoices { get; private set; } = [];
    public ICollection<SubscriptionCancellationLog> CancellationLogs { get; private set; } = [];

    private TenantSubscription() { }

    public static TenantSubscription Create(
        int tenantId,
        int solutionId,
        int subscriptionPlanId,
        int planPriceId,
        int statusId,
        DateTime startDate,
        DateTime? endDate = null,
        DateTime? trialEndDate = null,
        string? notes = null)
    {
        if (tenantId <= 0)
            throw new ArgumentException("Tenant ID must be greater than zero", nameof(tenantId));

        if (solutionId <= 0)
            throw new ArgumentException("Solution ID must be greater than zero", nameof(solutionId));

        if (subscriptionPlanId <= 0)
            throw new ArgumentException("Subscription plan ID must be greater than zero", nameof(subscriptionPlanId));

        if (planPriceId <= 0)
            throw new ArgumentException("Plan price ID must be greater than zero", nameof(planPriceId));

        if (statusId <= 0)
            throw new ArgumentException("Status ID must be greater than zero", nameof(statusId));

        if (endDate.HasValue && endDate.Value <= startDate)
            throw new ArgumentException("End date must be greater than start date", nameof(endDate));

        if (trialEndDate.HasValue && trialEndDate.Value <= startDate)
            throw new ArgumentException("Trial end date must be greater than start date", nameof(trialEndDate));

        var subscription = new TenantSubscription
        {
            TenantId = tenantId,
            SolutionId = solutionId,
            SubscriptionPlanId = subscriptionPlanId,
            PlanPriceId = planPriceId,
            StatusId = statusId,
            StartDate = startDate,
            EndDate = endDate,
            TrialEndDate = trialEndDate,
            Notes = notes?.Trim()
        };

        subscription.InitializeAudit();
        return subscription;
    }

    public void UpdateStatus(int statusId)
    {
        if (statusId <= 0)
            throw new ArgumentException("Status ID must be greater than zero", nameof(statusId));

        StatusId = statusId;
        MarkAsUpdated();
    }

    public void UpdatePlan(int subscriptionPlanId, int planPriceId)
    {
        if (subscriptionPlanId <= 0)
            throw new ArgumentException("Subscription plan ID must be greater than zero", nameof(subscriptionPlanId));

        if (planPriceId <= 0)
            throw new ArgumentException("Plan price ID must be greater than zero", nameof(planPriceId));

        SubscriptionPlanId = subscriptionPlanId;
        PlanPriceId = planPriceId;
        MarkAsUpdated();
    }

    /// <summary>
    /// Immediately cancels the subscription by setting EndDate.
    /// Used for: immediate cancellations and when the Stripe webhook confirms
    /// a period-end cancellation has completed.
    /// </summary>
    public void Cancel(DateTime endDate)
    {
        EndDate = endDate;
        CancellationScheduledAt = null; // Clear any pending schedule — now fully cancelled
        MarkAsUpdated();
    }

    /// <summary>
    /// Marks the subscription as scheduled for cancellation at the end of the
    /// current billing period. The subscription remains active until that date.
    /// CancellationScheduledAt being non-null signals to the UI that
    /// a cancellation is pending.
    /// </summary>
    public void ScheduleCancellation(DateTime scheduledAt)
    {
        CancellationScheduledAt = scheduledAt;
        MarkAsUpdated();
    }

    public void SetStripeSubscriptionId(string stripeSubscriptionId)
    {
        StripeSubscriptionId = stripeSubscriptionId?.Trim();
        MarkAsUpdated();
    }

    public void SetStripeSessionId(string stripeSessionId)
    {
        if (string.IsNullOrWhiteSpace(stripeSessionId))
            throw new ArgumentException("Stripe session ID is required", nameof(stripeSessionId));

        StripeSessionId = stripeSessionId.Trim();
        MarkAsUpdated();
    }

    public void UpdateNotes(string? notes) { Notes = notes?.Trim(); MarkAsUpdated(); }

    public bool IsExpired() => EndDate.HasValue && EndDate.Value < DateTime.UtcNow;

    public bool HasPendingCancellation() => CancellationScheduledAt.HasValue;
}
