using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

/// <summary>
/// Tenant-specific role that defines access permissions for users.
/// Can be created from a system template or created as a custom role.
/// </summary>
public class Role : TenantEntity
{
    /// <summary>
    /// Optional reference to the template this role was created from
    /// NULL for custom roles created by tenant admins
    /// </summary>
    public int? RoleTemplateId { get; private set; }

    /// <summary>
    /// Display name of the role
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Optional description of the role's purpose
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Indicates if this role has been customized by the tenant
    /// false = pristine copy from template
    /// true = either custom-created or template-based but modified
    /// </summary>
    public bool IsCustom { get; private set; }

    // Navigation properties
    /// <summary>
    /// Reference to the template this role was created from (if applicable)
    /// </summary>
    public RoleTemplate? RoleTemplate { get; private set; }

    /// <summary>
    /// Menu items accessible by this role
    /// </summary>
    public ICollection<RoleMenuItem> RoleMenuItems { get; private set; } = [];

    /// <summary>
    /// Action permissions for this role on menu items
    /// </summary>
    public ICollection<RoleMenuItemAction> RoleMenuItemActions { get; private set; } = [];

    /// <summary>
    /// Users assigned to this role
    /// </summary>
    public ICollection<UserRole> UserRoles { get; private set; } = [];

    private Role() { }

    /// <summary>
    /// Creates a new role for a tenant
    /// </summary>
    /// <param name="tenantId">ID of the tenant this role belongs to</param>
    /// <param name="name">Display name of the role</param>
    /// <param name="createdBy">ID of the user creating the role (null for system-created)</param>
    /// <param name="roleTemplateId">Optional ID of the template this role is based on</param>
    /// <param name="description">Optional description</param>
    /// <param name="isCustom">Whether this is a custom role (true) or template-based (false)</param>
    /// <returns>New Role instance</returns>
    public static Role Create(
        int tenantId,
        string name,
        int? createdBy,
        int? roleTemplateId = null,
        string? description = null,
        bool isCustom = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required", nameof(name));

        var role = new Role
        {
            RoleTemplateId = roleTemplateId,
            Name = name.Trim(),
            Description = description?.Trim(),
            IsCustom = isCustom
        };

        role.InitializeTenantEntity(tenantId, createdBy);
        return role;
    }

    /// <summary>
    /// Marks this role as customized (modified from original template)
    /// </summary>
    public void MarkAsCustomized()
    {
        if (!IsCustom)
        {
            IsCustom = true;
            RecordUpdate(null); // Use RecordUpdate from TenantEntity
        }
    }

    /// <summary>
    /// Validates that this role can be reset to its template
    /// Actual permission reset happens in application layer
    /// </summary>
    public void ResetToTemplate()
    {
        if (RoleTemplateId == null)
            throw new InvalidOperationException("Cannot reset custom role - no template reference");

        if (IsCustom)
        {
            IsCustom = false;
            RecordUpdate(null); // Use RecordUpdate from TenantEntity
        }
    }

    /// <summary>
    /// Updates role information
    /// </summary>
    public void Update(string name, string? description, int? updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        RecordUpdate(updatedBy);
    }
}
