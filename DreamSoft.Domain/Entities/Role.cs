using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class Role : TenantEntity
{
    // SolutionId is now inherited from TenantEntity
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = "";
    public TranslatedString Translations { get; private set; } = null!;
    public bool IsCustom { get; private set; }
    public int? RoleTemplateId { get; private set; }

    // Navigation properties
    public RoleTemplate? RoleTemplate { get; private set; }
    public ICollection<User> Users { get; private set; } = [];
    public ICollection<RoleMenuOption> RoleMenuOptions { get; private set; } = [];
    public ICollection<RoleOptionAction> RoleOptionActions { get; private set; } = [];

    private Role() { }

    public static Role Create(
        int tenantId,
        int solutionId,
        string code,
        string name,
        TranslatedString translations,
        string description = "",
        int? roleTemplateId = null,
        bool isCustom = false,
        int? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        var role = new Role
        {
            Code = code.ToUpper().Trim(),
            Name = name.Trim(),
            Description = description.Trim(),
            Translations = translations,
            RoleTemplateId = roleTemplateId,
            IsCustom = isCustom
        };

        role.InitializeTenantEntity(tenantId, solutionId, createdBy);
        return role;
    }

    public void UpdateDetails(string name, string description,
        TranslatedString translations, int? updatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        Name = name.Trim();
        Description = description.Trim();
        Translations = translations;
        RecordUpdate(updatedBy);
    }
}
