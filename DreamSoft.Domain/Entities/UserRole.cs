using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class UserRole : AuditableEntity
{
    public int UserId { get; protected set; }
    public int RoleId { get; protected set; }
    public int? CreatedBy { get; protected set; }
    public int? UpdatedBy { get; protected set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;
    public User? CreatedByUser { get; private set; }
    public User? UpdatedByUser { get; private set; }

    private UserRole() { }

    public static UserRole Create(int userId, int roleId, int? createdBy = null)
    {
        if (userId <= 0)
            throw new ArgumentException("User ID is required", nameof(userId));

        if (roleId <= 0)
            throw new ArgumentException("Role ID is required", nameof(roleId));

        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            CreatedBy = createdBy
        };

        userRole.InitializeAudit();
        return userRole;
    }
}
