namespace DreamSoft.Domain.Entities;

/// <summary>
/// System-wide junction table defining which actions role templates have on menu items.
/// Defines action-level permissions for role templates (e.g., Admin can VIEW, CREATE, EDIT, DELETE on Customers menu).
/// Not tenant-specific - shared across all tenants as templates.
/// </summary>
public class RoleMenuActionTemplate
{
    /// <summary>
    /// Foreign key to the role template
    /// Part of composite primary key
    /// </summary>
    public int RoleTemplateId { get; private set; }

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
    /// The role template this action permission belongs to
    /// </summary>
    public RoleTemplate RoleTemplate { get; private set; } = null!;

    /// <summary>
    /// The menu item this action applies to
    /// </summary>
    public MenuItem MenuItem { get; private set; } = null!;

    /// <summary>
    /// The action that can be performed (VIEW, CREATE, EDIT, DELETE, etc.)
    /// </summary>
    public PermissionAction PermissionAction { get; private set; } = null!;

    private RoleMenuActionTemplate() { }

    /// <summary>
    /// Creates a new role template action permission
    /// </summary>
    /// <param name="roleTemplateId">ID of the role template</param>
    /// <param name="menuItemId">ID of the menu item</param>
    /// <param name="actionId">ID of the permission action</param>
    /// <returns>New RoleMenuActionTemplate instance</returns>
    public static RoleMenuActionTemplate Create(int roleTemplateId, int menuItemId, int actionId)
    {
        if (roleTemplateId <= 0)
            throw new ArgumentException("Role template ID is required", nameof(roleTemplateId));

        if (menuItemId <= 0)
            throw new ArgumentException("Menu item ID is required", nameof(menuItemId));

        if (actionId <= 0)
            throw new ArgumentException("Action ID is required", nameof(actionId));

        return new RoleMenuActionTemplate
        {
            RoleTemplateId = roleTemplateId,
            MenuItemId = menuItemId,
            ActionId = actionId
        };
    }
}
