using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

/// <summary>
/// System-wide role template used to initialize roles for new tenants.
/// Not tenant-specific - shared across all tenants.
/// </summary>
public class RoleTemplate : LookupEntity
{
    /// <summary>
    /// Unique code identifier for the template (e.g., "ADMIN", "MANAGER", "EMPLOYEE")
    /// Used for programmatic reference and must be unique across all templates
    /// </summary>
    public string Code { get; private set; } = null!;

    /// <summary>
    /// Optional description of the role template
    /// </summary>
    public string? Description { get; private set; }

    // Navigation properties
    /// <summary>
    /// Menu items that should be assigned to roles created from this template
    /// </summary>
    public ICollection<RoleMenuItemTemplate> RoleMenuItemTemplates { get; private set; } = [];

    /// <summary>
    /// Action permissions that should be assigned to roles created from this template
    /// </summary>
    public ICollection<RoleMenuActionTemplate> RoleMenuActionTemplates { get; private set; } = [];

    /// <summary>
    /// Roles that were created from this template across all tenants
    /// </summary>
    public ICollection<Role> Roles { get; private set; } = [];

    private RoleTemplate() { }

    /// <summary>
    /// Creates a new role template
    /// </summary>
    /// <param name="code">Unique code identifier (will be converted to uppercase)</param>
    /// <param name="name">Display name in default language</param>
    /// <param name="description">Optional description</param>
    /// <param name="translations">Optional JSONB translations</param>
    /// <returns>New RoleTemplate instance</returns>
    public static RoleTemplate Create(
        string code,
        string name,
        TranslatedString translations,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Role template code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role template name is required", nameof(name));

        return new RoleTemplate
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Translations = translations,
            IsActive = true
        };
    }

    /// <summary>
    /// Updates template information
    /// </summary>
    public void Update(string name, TranslatedString translations, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role template name is required", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        Translations = translations;
        MarkAsUpdated();
    }
}
