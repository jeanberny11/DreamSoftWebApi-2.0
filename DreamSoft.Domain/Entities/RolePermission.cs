using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class RolePermission : AuditableEntity
{
    public int RoleId { get; protected set; }
    public int PermissionId { get; protected set; }
    public int? CreatedBy { get; protected set; }
    public int? UpdatedBy { get; protected set; }

    // Navigation properties
    public Role Role { get; private set; } = null!;
    public Permission Permission { get; private set; } = null!;
    public User? CreatedByUser { get; private set; }
    public User? UpdatedByUser { get; private set; }

    private RolePermission() { }

    public static RolePermission Create(int roleId, int permissionId, int? createdBy = null)
    {
        if (roleId <= 0)
            throw new ArgumentException("Role ID is required", nameof(roleId));

        if (permissionId <= 0)
            throw new ArgumentException("Permission ID is required", nameof(permissionId));

        var rolePermission = new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            CreatedBy = createdBy
        };

        rolePermission.InitializeAudit();
        return rolePermission;
    }
}
