namespace DreamSoft.Domain.Entities;

/// <summary>
/// Junction table linking tenant roles to menu items.
/// Defines which menu items each tenant's role can access.
/// Tenant-specific through the Role relationship.
/// </summary>
public class RoleMenuItem
{
    /// <summary>
    /// Foreign key to the role
    /// Part of composite primary key
    /// </summary>
    public int RoleId { get; private set; }

    /// <summary>
    /// Foreign key to the menu item
    /// Part of composite primary key
    /// </summary>
    public int MenuItemId { get; private set; }

    // Navigation properties
    /// <summary>
    /// The role this menu item is assigned to
    /// </summary>
    public Role Role { get; private set; } = null!;

    /// <summary>
    /// The menu item accessible by this role
    /// </summary>
    public MenuItem MenuItem { get; private set; } = null!;

    private RoleMenuItem() { }

    /// <summary>
    /// Creates a new role menu item permission
    /// </summary>
    /// <param name="roleId">ID of the role</param>
    /// <param name="menuItemId">ID of the menu item to grant access to</param>
    /// <returns>New RoleMenuItem instance</returns>
    public static RoleMenuItem Create(int roleId, int menuItemId)
    {
        if (roleId <= 0)
            throw new ArgumentException("Role ID is required", nameof(roleId));

        if (menuItemId <= 0)
            throw new ArgumentException("Menu item ID is required", nameof(menuItemId));

        return new RoleMenuItem
        {
            RoleId = roleId,
            MenuItemId = menuItemId
        };
    }
}
