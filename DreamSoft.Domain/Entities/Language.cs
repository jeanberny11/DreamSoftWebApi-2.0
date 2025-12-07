namespace DreamSoft.Domain.Entities;

public class Language : BaseEntity<int>
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string NativeName { get; private set; } = null!;
    public bool IsDefault { get; private set; }

    // Navigation property
    public ICollection<User> Users { get; private set; } = new List<User>();

    // Private constructor for EF Core
    private Language()
    {
    }

    /// <summary>
    /// Creates a new language (typically used for seeding)
    /// </summary>
    public static Language Create(
        string code,
        string name,
        string nativeName,
        bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(nativeName))
            throw new ArgumentException("Native name is required", nameof(nativeName));

        return new Language
        {
            Code = code.ToLower().Trim(),
            Name = name.Trim(),
            NativeName = nativeName.Trim(),
            IsDefault = isDefault,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates language information
    /// </summary>
    public void Update(string name, string nativeName)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(nativeName))
            throw new ArgumentException("Native name is required", nameof(nativeName));

        Name = name.Trim();
        NativeName = nativeName.Trim();
    }

    /// <summary>
    /// Sets as default language
    /// </summary>
    public void SetAsDefault()
    {
        IsDefault = true;
    }

    /// <summary>
    /// Removes default flag
    /// </summary>
    public void RemoveDefault()
    {
        IsDefault = false;
    }

    /// <summary>
    /// Activates the language
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the language
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
}