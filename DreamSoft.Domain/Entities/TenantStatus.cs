using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class TenantStatus:BaseEntity<int>
{
    public string Code { get; private set; } = null!;
    public TranslatedString Name { get; private set; } = null!;
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
    public static TenantStatus Create(string code, TranslatedString name, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        ArgumentNullException.ThrowIfNull(name);

        return new TenantStatus
        {
            Code = code.ToLower().Trim(),
            Name = name,
            IsActive = true,
            DisplayOrder = displayOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates the name translations
    /// </summary>
    public void UpdateName(TranslatedString name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates display order
    /// </summary>
    public void UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the status
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the status (won't be available for new tenants)
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}