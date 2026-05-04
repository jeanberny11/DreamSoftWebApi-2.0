using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class MenuOption : LookupEntity
{
    public string Code { get; set; } = null!;
    public string Description { get; set; } = "";
    public int ModuleId { get; set; }
    public int MenuGroupId { get; set; }
    public string Route { get; set; } = "";
    public string Icon { get; set; } = "";
    public int SortOrder { get; set; }

    // Navigation properties
    public Module Module { get; set; } = null!;
    public MenuGroup MenuGroup { get; set; } = null!;
    public ICollection<PlanMenuOption> PlanMenuOptions { get; private set; } = [];
    public ICollection<RoleMenuOption> RoleMenuOptions { get; private set; } = [];
    public ICollection<RoleMenuOptionTemplate> RoleMenuOptionTemplates { get; private set; } = [];
    public ICollection<RoleOptionAction> RoleOptionActions { get; private set; } = [];
    public ICollection<RoleOptionActionTemplate> RoleOptionActionTemplates { get; private set; } = [];

    private MenuOption() { }

    public static MenuOption Create(string code, string name, TranslatedString translations,
        int moduleId, int menuGroupId, string description = "",
        string route = "", string icon = "", int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        if (moduleId <= 0)
            throw new ArgumentException("Module ID must be greater than zero", nameof(moduleId));

        if (menuGroupId <= 0)
            throw new ArgumentException("Menu group ID must be greater than zero", nameof(menuGroupId));

        var menuOption = new MenuOption
        {
            Code = code.ToUpper().Trim(),
            Name = name.Trim(),
            Description = description.Trim(),
            Translations = translations,
            ModuleId = moduleId,
            MenuGroupId = menuGroupId,
            Route = route.Trim(),
            Icon = icon.Trim(),
            SortOrder = sortOrder
        };

        menuOption.InitializeAudit();
        return menuOption;
    }

    public void UpdateDetails(string name, string description, TranslatedString translations)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        Name = name.Trim();
        Description = description.Trim();
        UpdateTranslations(translations);
    }

    public void UpdateRoute(string route) { Route = route.Trim(); MarkAsUpdated(); }
    public void UpdateIcon(string icon) { Icon = icon.Trim(); MarkAsUpdated(); }
    public void UpdateSortOrder(int sortOrder) { SortOrder = sortOrder; MarkAsUpdated(); }

    public void MoveToMenuGroup(int menuGroupId)
    {
        if (menuGroupId <= 0)
            throw new ArgumentException("Menu group ID must be greater than zero", nameof(menuGroupId));

        MenuGroupId = menuGroupId;
        MarkAsUpdated();
    }

    public void MoveToModule(int moduleId)
    {
        if (moduleId <= 0)
            throw new ArgumentException("Module ID must be greater than zero", nameof(moduleId));

        ModuleId = moduleId;
        MarkAsUpdated();
    }
}
