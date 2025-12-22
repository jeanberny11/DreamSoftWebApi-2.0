using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

/// <summary>
/// Lookup table for permission action types (VIEW, CREATE, EDIT, DELETE, EXPORT, IMPORT, APPROVE).
/// Defines what actions can be performed on menu items.
/// </summary>
public class PermissionAction : LookupEntity
{
    /// <summary>
    /// Unique code identifier for the action (e.g., "VIEW", "CREATE", "EDIT", "DELETE")
    /// Used for programmatic reference and must be unique across all actions
    /// </summary>
    public string Code { get; private set; } = null!;

    /// <summary>
    /// Optional description of the action's purpose
    /// </summary>
    public string? Description { get; private set; }

    // Navigation properties
    /// <summary>
    /// Template-level action permissions that reference this action
    /// </summary>
    public ICollection<RoleMenuActionTemplate> RoleMenuActionTemplates { get; private set; } = [];

    /// <summary>
    /// Tenant-level action permissions that reference this action
    /// </summary>
    public ICollection<RoleMenuItemAction> RoleMenuItemActions { get; private set; } = [];

    private PermissionAction() { }

    /// <summary>
    /// Creates a new permission action
    /// </summary>
    /// <param name="code">Unique code identifier (will be converted to uppercase)</param>
    /// <param name="name">Display name in default language</param>
    /// <param name="translations">JSONB translations for name</param>
    /// <param name="description">Optional description</param>
    /// <returns>New PermissionAction instance</returns>
    public static PermissionAction Create(
        string code,
        string name,
        TranslatedString translations,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations);

        var permissionAction = new PermissionAction
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Translations = translations
        };

        permissionAction.InitializeAudit();
        return permissionAction;
    }

    /// <summary>
    /// Updates permission action information
    /// </summary>
    public void Update(
        string name,
        TranslatedString translations,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations);

        Name = name.Trim();
        Description = description?.Trim();
        Translations = translations;
        MarkAsUpdated();
    }
}
