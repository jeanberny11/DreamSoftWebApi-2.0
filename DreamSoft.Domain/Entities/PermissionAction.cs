using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class PermissionAction : LookupEntity
{
    // Navigation properties
    public ICollection<Permission> Permissions { get; private set; } = [];

    private PermissionAction() { }

    public static PermissionAction Create(string name, TranslatedString translations)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations);

        var permissionAction = new PermissionAction
        {
            Name = name.Trim(),
            Translations = translations
        };

        permissionAction.InitializeAudit();
        return permissionAction;
    }
}
