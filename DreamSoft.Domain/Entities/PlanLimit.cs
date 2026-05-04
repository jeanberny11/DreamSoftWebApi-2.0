using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class PlanLimit : AuditableEntity
{
    public int PlanId { get; private set; }
    public string LimitKey { get; private set; } = null!;
    public decimal LimitValue { get; private set; }
    public string Description { get; private set; } = "";

    // Navigation properties
    public SubscriptionPlan Plan { get; private set; } = null!;

    private PlanLimit() { }

    public static PlanLimit Create(int planId, string limitKey, decimal limitValue, string description = "")
    {
        if (planId <= 0)
            throw new ArgumentException("Plan ID must be greater than zero", nameof(planId));

        if (string.IsNullOrWhiteSpace(limitKey))
            throw new ArgumentException("Limit key is required", nameof(limitKey));

        if (limitValue < 0)
            throw new ArgumentException("Limit value cannot be negative", nameof(limitValue));

        var limit = new PlanLimit
        {
            PlanId = planId,
            LimitKey = limitKey.ToLower().Trim(),
            LimitValue = limitValue,
            Description = description.Trim()
        };

        limit.InitializeAudit();
        return limit;
    }

    public void UpdateValue(decimal limitValue, string description)
    {
        if (limitValue < 0)
            throw new ArgumentException("Limit value cannot be negative", nameof(limitValue));

        LimitValue = limitValue;
        Description = description.Trim();
        MarkAsUpdated();
    }
}
