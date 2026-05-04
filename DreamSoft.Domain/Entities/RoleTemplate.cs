using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class RoleTemplate : LookupEntity
{
    public string Code { get; set; } = null!;
    public string Description { get; set; } = "";
    public int PlanId { get; set; }

    // Navigation properties
    public SubscriptionPlan Plan { get; private set; } = null!;
    public ICollection<Role> Roles { get; private set; } = [];
    public ICollection<RoleMenuOptionTemplate> RoleMenuOptionTemplates { get; private set; } = [];
    public ICollection<RoleOptionActionTemplate> RoleOptionActionTemplates { get; private set; } = [];

    private RoleTemplate() { }

    public static RoleTemplate Create(int planId, string code, string name,
        TranslatedString translations, string description = "")
    {
        if (planId <= 0)
            throw new ArgumentException("Plan ID must be greater than zero", nameof(planId));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        var roleTemplate = new RoleTemplate
        {
            PlanId = planId,
            Code = code.ToUpper().Trim(),
            Name = name.Trim(),
            Description = description.Trim(),
            Translations = translations
        };

        roleTemplate.InitializeAudit();
        return roleTemplate;
    }

    public void UpdateDescription(string description)
    {
        Description = description.Trim();
        MarkAsUpdated();
    }
}
