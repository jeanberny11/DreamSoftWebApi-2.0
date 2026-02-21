using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Common;

/// <summary>
/// Base entity for lookup/reference tables
/// Provides name (English fallback) and translations (multi-language) fields
/// Used for shared reference data across all tenants
/// Examples: CustomerTypes, ProductTypes, Genders, Languages
/// </summary>
public abstract class LookupEntity : AuditableEntity
{
    /// <summary>
    /// Official English name/description - serves as fallback if translations fail
    /// Example: "Individual", "Business", "Active", "Male"
    /// </summary>
    public string Name { get; protected set; } = string.Empty;

    /// <summary>
    /// Multi-language translations stored as JSONB
    /// Format: {"en":"English text","es":"Texto español"}
    /// Spanish is required, English is optional
    /// NOTE: Changed to required (not nullable) to match database constraints
    /// </summary>
    public TranslatedString Translations { get; protected set; } = null!;

    /// <summary>
    /// Gets the translated name for the specified language
    /// Falls back to Name field if translation is not available
    /// </summary>
    /// <param name="language">Language code (es, en)</param>
    /// <returns>Translated name or English fallback</returns>
    public string GetTranslatedName(string language = "es")
    {
        if (Translations == null)
            return Name;

        return Translations.GetNameOrFallback(language, Name);
    }

    /// <summary>
    /// Updates the translations
    /// </summary>
    protected void UpdateTranslations(TranslatedString translations)
    {
        ArgumentNullException.ThrowIfNull(translations, nameof(translations));
        Translations = translations;
        MarkAsUpdated();
    }
}
