using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class Permission : AuditableEntity
{
    public int MenuItemId { get; protected set; }
    public int ActionId { get; protected set; }
    public string? Description { get; protected set; }
    public TranslatedString? Translations { get; protected set; }

    // Navigation properties
    public MenuItem MenuItem { get; private set; } = null!;
    public PermissionAction Action { get; private set; } = null!;
    public ICollection<RolePermission> RolePermissions { get; private set; } = [];

    private Permission() { }

    public static Permission Create(int menuItemId, int actionId, string? description = null, TranslatedString? translations = null)
    {
        if (menuItemId <= 0)
            throw new ArgumentException("Menu item ID is required", nameof(menuItemId));

        if (actionId <= 0)
            throw new ArgumentException("Action ID is required", nameof(actionId));

        var permission = new Permission
        {
            MenuItemId = menuItemId,
            ActionId = actionId,
            Description = description?.Trim(),
            Translations = translations
        };

        permission.InitializeAudit();
        return permission;
    }
}
