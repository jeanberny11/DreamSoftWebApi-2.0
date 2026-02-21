namespace DreamSoft.Domain.Entities;

/// <summary>
/// Explicit join entity for the User ↔ Role many-to-many relationship.
/// Carries extra payload: who assigned the role and when.
/// </summary>
public class UserRole
{
    /// <summary>FK to the user being assigned the role.</summary>
    public int UserId { get; private set; }

    /// <summary>FK to the role being assigned.</summary>
    public int RoleId { get; private set; }

    /// <summary>UTC timestamp when the role was assigned.</summary>
    public DateTime AssignedAt { get; private set; }

    /// <summary>
    /// User ID of the actor who performed the assignment.
    /// Null when assigned by the system (e.g. during tenant seeding).
    /// </summary>
    public int? AssignedBy { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;
    public User? AssignedByUser { get; private set; }

    private UserRole() { }

    /// <summary>
    /// Factory method — creates a new role assignment.
    /// </summary>
    public static UserRole Create(int userId, int roleId, int? assignedBy = null)
    {
        if (userId <= 0)
            throw new ArgumentException("User ID must be greater than zero.", nameof(userId));

        if (roleId <= 0)
            throw new ArgumentException("Role ID must be greater than zero.", nameof(roleId));

        return new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = assignedBy
        };
    }
}
