using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class TenantSubscription : AuditableEntity
{
    public int TenantId { get; set; }
    public int SolutionId { get; set; }
    public int SubscriptionPlanId { get; set; }
    public int StatusId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public Solution Solution { get; set; } = null!;
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    public SubscriptionStatus Status { get; set; } = null!;

    private TenantSubscription() { }

    public static TenantSubscription Create(
        int tenantId,
        int solutionId,
        int subscriptionPlanId,
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

    public void UpdateEndDate(DateTime? endDate)
    {
        if (endDate.HasValue && endDate.Value <= StartDate)
            throw new ArgumentException("End date must be greater than start date", nameof(endDate));

        EndDate = endDate;
        MarkAsUpdated();
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
        MarkAsUpdated();
    }
    public bool IsExpired() => EndDate.HasValue && EndDate.Value < DateTime.UtcNow;
}
