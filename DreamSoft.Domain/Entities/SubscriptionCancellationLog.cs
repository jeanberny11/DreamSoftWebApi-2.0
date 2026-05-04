using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

/// <summary>
/// Audit log for subscription cancellation events.
/// One record is written per cancellation request, preserving the reason
/// and feedback even if the subscription is later reactivated.
/// CancellationType: "immediate" | "at_period_end"
/// </summary>
public class SubscriptionCancellationLog : AuditableEntity
{
    public int TenantSubscriptionId { get; private set; }
    public int TenantId { get; private set; }

    /// <summary>
    /// UTC timestamp when the cancellation was requested by the tenant.
    /// </summary>
    public DateTime CancelledAt { get; private set; }

    /// <summary>
    /// "immediate" or "at_period_end"
    /// </summary>
    public string CancellationType { get; private set; } = null!;

    /// <summary>
    /// Short reason code from the frontend (e.g. "too_expensive", "missing_feature").
    /// Null if the tenant skipped the reason step.
    /// </summary>
    public string? CancellationReason { get; private set; }

    /// <summary>
    /// Optional free-text feedback from the tenant.
    /// </summary>
    public string? CancellationFeedback { get; private set; }

    /// <summary>
    /// The date the subscription is/was scheduled to end.
    /// For immediate cancellations this equals CancelledAt.
    /// For period-end cancellations this is the billing period end date from Stripe.
    /// </summary>
    public DateTime? ScheduledEndDate { get; private set; }

    // Navigation properties
    public TenantSubscription TenantSubscription { get; private set; } = null!;
    public Tenant Tenant { get; private set; } = null!;

    private SubscriptionCancellationLog() { }

    public static SubscriptionCancellationLog Create(
        int tenantSubscriptionId,
        int tenantId,
        string cancellationType,
        DateTime cancelledAt,
        DateTime? scheduledEndDate = null,
        string? cancellationReason = null,
        string? cancellationFeedback = null)
    {
        if (tenantSubscriptionId <= 0)
            throw new ArgumentException("Tenant subscription ID must be greater than zero", nameof(tenantSubscriptionId));

        if (tenantId <= 0)
            throw new ArgumentException("Tenant ID must be greater than zero", nameof(tenantId));

        if (string.IsNullOrWhiteSpace(cancellationType))
            throw new ArgumentException("Cancellation type is required", nameof(cancellationType));

        var log = new SubscriptionCancellationLog
        {
            TenantSubscriptionId = tenantSubscriptionId,
            TenantId             = tenantId,
            CancellationType     = cancellationType.ToLower().Trim(),
            CancelledAt          = cancelledAt,
            ScheduledEndDate     = scheduledEndDate,
            CancellationReason   = cancellationReason?.Trim(),
            CancellationFeedback = cancellationFeedback?.Trim()
        };

        log.InitializeAudit();
        return log;
    }
}
