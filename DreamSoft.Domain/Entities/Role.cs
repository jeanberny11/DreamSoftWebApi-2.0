using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class Role : TenantEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = "";
    public TranslatedString Translations { get; protected set; } = null!;
    public bool IsCustom { get; set; }
    public int? RoleTemplateId { get; set; }

    // Navigation properties
    public RoleTemplate? RoleTemplate { get; set; }
    public ICollection<UserRole> UserRoles { get; private set; } = [];
    public ICollection<RoleMenuOption> RoleMenuOptions { get; private set; } = [];
    public ICollection<RoleOptionAction> RoleOptionActions { get; private set; } = [];

    private Role() { }

    public static Role Create(
        int tenantId,
        string code,
        string name,
        TranslatedString translatedString,
        string description = "",
        int? roleTemplateId = null,
        int? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        var role = new Role
        {
            Code = code.ToUpper().Trim(),
            Name = name.Trim(),
            Description = description.Trim(),
            RoleTemplateId = roleTemplateId,
            Translations = translatedString
        };

        role.InitializeTenantEntity(tenantId, createdBy);
        return role;
    }

    public void UpdateDetails(string name, string description, int? updatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        Name = name.Trim();
        Description = description.Trim();
        RecordUpdate(updatedBy);
    }

    public void AssignTemplate(int roleTemplateId, int? updatedBy = null)
    {
        if (roleTemplateId <= 0)
            throw new ArgumentException("Role template ID must be greater than zero", nameof(roleTemplateId));

        RoleTemplateId = roleTemplateId;
        RecordUpdate(updatedBy);
    }

    public void RemoveTemplate(int? updatedBy = null)
    {
        RoleTemplateId = null;
        RecordUpdate(updatedBy);
    }
}
