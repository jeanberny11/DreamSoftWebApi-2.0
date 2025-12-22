using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

/// <summary>
/// Junction table for User-Role many-to-many relationship
/// Uses composite primary key (UserId, RoleId) - no separate Id column
/// </summary>
public class UserRole
{
    public int UserId { get; protected set; }
    public int RoleId { get; protected set; }
    public int? CreatedBy { get; protected set; }
    public int? UpdatedBy { get; protected set; }
    
    // Audit fields (not inheriting from AuditableEntity)
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public bool IsActive { get; protected set; }

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
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            IsActive = true
        };

        return userRole;
    }

    public void Deactivate(int? updatedBy = null)
    {
        IsActive = false;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate(int? updatedBy = null)
    {
        IsActive = true;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}
