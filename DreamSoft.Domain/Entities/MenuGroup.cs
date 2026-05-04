using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class MenuGroup : LookupEntity
{
    public string Code { get; set; } = null!;
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "";
    public int SortOrder { get; set; }

    // Navigation properties
    public ICollection<MenuOption> MenuOptions { get; private set; } = [];

    private MenuGroup() { }

    public static MenuGroup Create(string code, string name, TranslatedString translations,
        string description = "", string icon = "", int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        var menuGroup = new MenuGroup
        {
            Code = code.ToUpper().Trim(),
            Name = name.Trim(),
            Description = description.Trim(),
            Translations = translations,
            Icon = icon.Trim(),
            SortOrder = sortOrder
        };

        menuGroup.InitializeAudit();
        return menuGroup;
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

    public void UpdateIcon(string icon) { Icon = icon.Trim(); MarkAsUpdated(); }
    public void UpdateSortOrder(int sortOrder) { SortOrder = sortOrder; MarkAsUpdated(); }
}
