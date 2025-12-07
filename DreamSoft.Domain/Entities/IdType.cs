using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class IdType : LookupEntity
{
    public int CountryId { get; private set; }
    public string Code { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? ValidationPattern { get; private set; }

    // Navigation property
    public ICollection<User> Users { get; private set; } = new List<User>();

    // Private constructor for EF Core
    private IdType()
    {
    }

    /// <summary>
    /// Creates a new ID type (typically used for seeding)
    /// </summary>
    public static IdType Create(
        int countryId,
        string code,
        string name,
        string? description = null,
        string? validationPattern = null,
        TranslatedString? translations = null)
    {
        if (countryId <= 0)
            throw new ArgumentException("Country ID must be valid", nameof(countryId));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        var idType = new IdType
        {
            CountryId = countryId,
            Code = code.ToUpper().Trim(),
            Name = name.Trim(),
            Description = description?.Trim(),
            ValidationPattern = validationPattern?.Trim(),
            Translations = translations
        };

        idType.InitializeAudit(); // Initialize base audit fields

        return idType;
    }

    /// <summary>
    /// Updates ID type information
    /// </summary>
    public void Update(
        string name,
        string? description = null,
        string? validationPattern = null,
        TranslatedString? translations = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        ValidationPattern = validationPattern?.Trim();
        UpdateTranslations(translations);
    }

    /// <summary>
    /// Validates an ID number against the pattern
    /// </summary>
    public bool ValidateIdNumber(string idNumber)
    {
        if (string.IsNullOrWhiteSpace(ValidationPattern))
            return true; // No validation pattern defined

        if (string.IsNullOrWhiteSpace(idNumber))
            return false;

        return System.Text.RegularExpressions.Regex.IsMatch(idNumber, ValidationPattern);
    }
}