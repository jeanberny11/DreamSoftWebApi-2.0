using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class MenuItem : LookupEntity
{
    public int ModuleId { get; protected set; }
    public int MenuGroupId { get; protected set; }
    public string Code { get; protected set; } = null!;
    public string? Description { get; protected set; }
    public string? Route { get; protected set; }
    public string? Icon { get; protected set; }
    public int? RequiredTierId { get; protected set; }
    public int? SortOrder { get; protected set; }

    // Navigation properties
    public Module Module { get; private set; } = null!;
    public MenuGroup MenuGroup { get; private set; } = null!;
    public SubscriptionTier? RequiredTier { get; private set; }
    public ICollection<Permission> Permissions { get; private set; } = [];

    private MenuItem() { }

    public static MenuItem Create(int moduleId, int menuGroupId, string code, string name, TranslatedString? translations = null, string? description = null, string? route = null, string? icon = null, int? requiredTierId = null, int? sortOrder = null)
    {
        if (moduleId <= 0)
            throw new ArgumentException("Module ID is required", nameof(moduleId));

        if (menuGroupId <= 0)
            throw new ArgumentException("Menu group ID is required", nameof(menuGroupId));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        var menuItem = new MenuItem
        {
            ModuleId = moduleId,
            MenuGroupId = menuGroupId,
            Code = code.ToLower().Trim(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Route = route?.Trim(),
            Icon = icon?.Trim(),
            RequiredTierId = requiredTierId,
            SortOrder = sortOrder,
            Translations = translations
        };

        menuItem.InitializeAudit();
        return menuItem;
    }
}
