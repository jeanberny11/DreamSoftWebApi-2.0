using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class Gender:BaseEntity<int>
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public TranslatedString? Translations { get; private set; }

    // Navigation property
    public ICollection<User> Users { get; private set; } = new List<User>();

    // Private constructor for EF Core
    private Gender()
    {
    }

    /// <summary>
    /// Creates a new gender (typically used for seeding)
    /// </summary>
    public static Gender Create(string code, string name, TranslatedString? translations = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        return new Gender
        {
            Code = code.ToUpper().Trim(),
            Name = name.Trim(),
            Translations = translations,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates translations
    /// </summary>
    public void UpdateTranslations(TranslatedString translations)
    {
        Translations = translations;
    }

    /// <summary>
    /// Activates the gender
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the gender
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
}