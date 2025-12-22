namespace DreamSoft.Domain.Entities;

/// <summary>
/// Tenant-specific junction table defining which actions roles have on menu items.
/// Defines action-level permissions for tenant roles (e.g., Cashier can VIEW, CREATE, EDIT but not DELETE customers).
/// Tenant-specific through the Role relationship.
/// </summary>
public class RoleMenuItemAction
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

    /// <summary>
    /// Foreign key to the permission action (VIEW, CREATE, EDIT, DELETE, etc.)
    /// Part of composite primary key
    /// </summary>
    public int ActionId { get; private set; }

    // Navigation properties
    /// <summary>
    /// The role this action permission belongs to
    /// </summary>
    public Role Role { get; private set; } = null!;

    /// <summary>
    /// The menu item this action applies to
    /// </summary>
    public MenuItem MenuItem { get; private set; } = null!;

    /// <summary>
    /// The action that can be performed (VIEW, CREATE, EDIT, DELETE, etc.)
    /// </summary>
    public PermissionAction PermissionAction { get; private set; } = null!;

    private RoleMenuItemAction() { }

    /// <summary>
    /// Creates a new role action permission
    /// </summary>
    /// <param name="roleId">ID of the role</param>
    /// <param name="menuItemId">ID of the menu item</param>
    /// <param name="actionId">ID of the permission action</param>
    /// <returns>New RoleMenuItemAction instance</returns>
    public static RoleMenuItemAction Create(int roleId, int menuItemId, int actionId)
    {
        if (roleId <= 0)
            throw new ArgumentException("Role ID is required", nameof(roleId));

        if (menuItemId <= 0)
            throw new ArgumentException("Menu item ID is required", nameof(menuItemId));

        if (actionId <= 0)
            throw new ArgumentException("Action ID is required", nameof(actionId));

        return new RoleMenuItemAction
        {
            RoleId = roleId,
            MenuItemId = menuItemId,
            ActionId = actionId
        };
    }
}
