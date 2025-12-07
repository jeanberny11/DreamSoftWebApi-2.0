using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class Gender : LookupEntity
{
    public string Code { get; private set; } = null!;

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

        var gender = new Gender
        {
            Code = code.ToUpper().Trim(),
            Name = name.Trim(),
            Translations = translations
        };

        gender.InitializeAudit(); // Initialize base audit fields

        return gender;
    }

    /// <summary>
    /// Updates translations
    /// </summary>
    public void SetTranslations(TranslatedString? translations)
    {
        UpdateTranslations(translations);
    }
}