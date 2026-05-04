using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class SubscriptionPlan : LookupEntity
{
    public string Code { get; set; } = null!;
    public string Description { get; set; } = "";
    public int SolutionId { get; set; }
    public int TrialDays { get; set; }
    public int TierLevel { get; set; }

    // Navigation properties
    public Solution Solution { get; set; } = null!;
    public ICollection<PlanPrice> PlanPrices { get; private set; } = [];
    public ICollection<PlanLimit> PlanLimits { get; private set; } = [];
    public ICollection<PlanMenuOption> PlanMenuOptions { get; private set; } = [];
    public ICollection<RoleTemplate> RoleTemplates { get; private set; } = [];
    public ICollection<TenantSubscription> TenantSubscriptions { get; private set; } = [];

    private SubscriptionPlan() { }

    public static SubscriptionPlan Create(string code, string name, TranslatedString translations,
        int solutionId, int tierLevel, string description = "", int trialDays = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        if (solutionId <= 0)
            throw new ArgumentException("Solution ID must be greater than zero", nameof(solutionId));

        if (tierLevel <= 0)
            throw new ArgumentException("Tier level must be greater than zero", nameof(tierLevel));

        if (trialDays < 0)
            throw new ArgumentException("Trial days cannot be negative", nameof(trialDays));

        var plan = new SubscriptionPlan
        {
            Code = code.ToUpper().Trim(),
            Name = name.Trim(),
            Description = description.Trim(),
            Translations = translations,
            SolutionId = solutionId,
            TierLevel = tierLevel,
            TrialDays = trialDays,
            IsActive = isActive
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

    public void UpdateTrialDays(int trialDays)
    {
        if (trialDays < 0)
            throw new ArgumentException("Trial days cannot be negative", nameof(trialDays));

        TrialDays = trialDays;
        MarkAsUpdated();
    }

    public void SetActive(bool isActive) { IsActive = isActive; MarkAsUpdated(); }
    public bool HasTrial() => TrialDays > 0;
}
