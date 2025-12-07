using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class IdType : BaseEntity<int>
{
    public int CountryId { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? ValidationPattern { get; private set; }
    public TranslatedString? Translations { get; private set; }

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

        return new IdType
        {
            CountryId = countryId,
            Code = code.ToUpper().Trim(),
            Name = name.Trim(),
            Description = description?.Trim(),
            ValidationPattern = validationPattern?.Trim(),
            Translations = translations,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
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
        Translations = translations;
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

    /// <summary>
    /// Activates the ID type
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the ID type
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
}