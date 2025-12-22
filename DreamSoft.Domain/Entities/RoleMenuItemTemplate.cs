namespace DreamSoft.Domain.Entities;

/// <summary>
/// Junction table linking role templates to menu items.
/// Defines which menu items should be assigned to roles created from each template.
/// System-wide entity - not tenant-specific.
/// </summary>
public class RoleMenuItemTemplate
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

    // Navigation properties
    /// <summary>
    /// The role template this menu item belongs to
    /// </summary>
    public RoleTemplate RoleTemplate { get; private set; } = null!;

    /// <summary>
    /// The menu item assigned to this role template
    /// </summary>
    public MenuItem MenuItem { get; private set; } = null!;

    private RoleMenuItemTemplate() { }

    /// <summary>
    /// Creates a new role template menu item assignment
    /// </summary>
    /// <param name="roleTemplateId">ID of the role template</param>
    /// <param name="menuItemId">ID of the menu item to assign</param>
    /// <returns>New RoleMenuItemTemplate instance</returns>
    public static RoleMenuItemTemplate Create(int roleTemplateId, int menuItemId)
    {
        if (roleTemplateId <= 0)
            throw new ArgumentException("Role template ID is required", nameof(roleTemplateId));

        if (menuItemId <= 0)
            throw new ArgumentException("Menu item ID is required", nameof(menuItemId));

        return new RoleMenuItemTemplate
        {
            RoleTemplateId = roleTemplateId,
            MenuItemId = menuItemId
        };
    }
}
