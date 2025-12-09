using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class TenantStatus : LookupEntity
{
    public int DisplayOrder { get; private set; }

    // Navigation property
    public ICollection<Tenant> Tenants { get; private set; } = new List<Tenant>();

    // Private constructor for EF Core
    private TenantStatus()
    {
    }

    /// <summary>
    /// Creates a new tenant status (typically only used for seeding)
    /// </summary>
    public static TenantStatus Create(string code, string name, TranslatedString? translations, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        var tenantStatus = new TenantStatus
        {
            Name = name.Trim(),
            Translations = translations,
            DisplayOrder = displayOrder
        };

        tenantStatus.InitializeAudit(); // Initialize base audit fields

        return tenantStatus;
    }

    /// <summary>
    /// Updates the name and translations
    /// </summary>
    public void UpdateName(string name, TranslatedString? translations = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        Name = name.Trim();
        UpdateTranslations(translations);
    }

    /// <summary>
    /// Updates display order
    /// </summary>
    public void UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
        MarkAsUpdated();
    }
}