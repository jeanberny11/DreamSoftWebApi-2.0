using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

/// <summary>
/// Lookup table: Customer type options (e.g. Individual, Business).
/// Shared reference data across all tenants.
/// </summary>
public class CustomerType : LookupEntity
{
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    // Navigation properties
    public ICollection<Customer> Customers { get; private set; } = [];

    private CustomerType() { }

    public static CustomerType Create(string name, string code, TranslatedString translations, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        var customerType = new CustomerType
        {
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
            Description = description?.Trim(),
            Translations = translations
        };

        customerType.InitializeAudit();
        return customerType;
    }

    public void Update(string name, string code, TranslatedString translations, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
        Description = description?.Trim();
        UpdateTranslations(translations);
    }
}
