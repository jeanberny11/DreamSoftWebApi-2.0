using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class MenuGroup : LookupEntity
{
    public string Code { get; protected set; } = null!;
    public string? Description { get; protected set; }
    public string? Icon { get; protected set; }
    public int? SortOrder { get; protected set; }

    // Navigation properties
    public ICollection<MenuItem> MenuItems { get; private set; } = [];

    private MenuGroup() { }

    public static MenuGroup Create(string code, string name, TranslatedString? translations = null, string? description = null, string? icon = null, int? sortOrder = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        var menuGroup = new MenuGroup
        {
            Code = code.ToLower().Trim(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Icon = icon?.Trim(),
            SortOrder = sortOrder,
            Translations = translations
        };

        menuGroup.InitializeAudit();
        return menuGroup;
    }
}
