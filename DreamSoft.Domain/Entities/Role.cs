using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class Role : TenantEntity
{
    public string RoleName { get; protected set; } = null!;
    public string? Description { get; protected set; }
    public bool IsSystemRole { get; protected set; }

    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; private set; } = [];
    public ICollection<UserRole> UserRoles { get; private set; } = [];

    private Role() { }

    public static Role Create(int tenantId, string roleName, int? createdBy, string? description = null, bool isSystemRole = false)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ArgumentException("Role name is required", nameof(roleName));

        var role = new Role
        {
            RoleName = roleName.Trim(),
            Description = description?.Trim(),
            IsSystemRole = isSystemRole
        };

        role.InitializeTenantEntity(tenantId, createdBy);
        return role;
    }
}
