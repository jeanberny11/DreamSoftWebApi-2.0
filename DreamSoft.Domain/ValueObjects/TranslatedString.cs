using System.Text.Json.Serialization;

namespace DreamSoft.Domain.ValueObjects;

public class TranslatedString : ValueObject
{
    [JsonPropertyName("es")]
    public BaseTranslatedProperties Spanish { get; private set; } = null!;

    [JsonPropertyName("en")]
    public BaseTranslatedProperties? English { get; private set; }

    // Private constructor for EF Core
    private TranslatedString()
    {
    }

    // Factory method for creating instances
    public static TranslatedString Create(BaseTranslatedProperties spanish, BaseTranslatedProperties? english = null)
    {
        if (spanish == null)
            throw new ArgumentException("Spanish translation is required", nameof(spanish));

        return new TranslatedString
        {
            Spanish = spanish,
            English = english
        };
    }

    /// <summary>
    /// Gets the translation in the specified language
    /// </summary>
    /// <param name="language">Language code (es, en)</param>
    /// <param name="fallbackToSpanish">If true, returns Spanish when English is not available</param>
    public BaseTranslatedProperties? Get(string language = "es", bool fallbackToSpanish = true)
    {
        return language?.ToLower() switch
        {
            "en" when English != null => English,
            "en" when fallbackToSpanish => Spanish,
            "en" => null, // Return empty if no fallback
            _ => Spanish
        };
    }

    /// <summary>
    /// Gets the name translation in the specified language
    /// </summary>
    /// <param name="language">Language code (es, en)</param>
    /// <param name="fallbackToSpanish">If true, returns Spanish when English is not available</param>
    public string GetName(string language = "es", bool fallbackToSpanish = true)
    {
        return language?.ToLower() switch
        {
            "en" when English != null => English.Name,
            "en" when fallbackToSpanish => Spanish.Name,
            "en" => string.Empty, // Return empty if no fallback
            _ => Spanish.Name
        };
    }

    /// <summary>
    /// Gets the descripcion translation in the specified language
    /// </summary>
    /// <param name="language">Language code (es, en)</param>
    /// <param name="fallbackToSpanish">If true, returns Spanish when English is not available</param>
    public string? GetDescription(string language = "es", bool fallbackToSpanish = true)
    {
        return language?.ToLower() switch
        {
            "en" when English != null => English.Descripcion,
            "en" when fallbackToSpanish => Spanish.Descripcion,
            "en" => string.Empty, // Return empty if no fallback
            _ => Spanish.Descripcion
        };
    }

    /// <summary>
    /// Gets translation with name as ultimate fallback
    /// </summary>
    /// <param name="language">Language code (es, en)</param>
    /// <param name="fallbackName">Fallback name to use if translation is not available</param>
    /// <returns>Translation or fallback name</returns>
    public string GetNameOrFallback(string language, string fallbackName)
    {
        var translation = GetName(language, fallbackToSpanish: false);
        return string.IsNullOrEmpty(translation) ? fallbackName : translation;
    }

    /// <summary>
    /// Gets translation with Description as ultimate fallback
    /// </summary>
    /// <param name="language">Language code (es, en)</param>
    /// <param name="fallbackName">Fallback name to use if translation is not available</param>
    /// <returns>Translation or fallback Description</returns>
    public string GetDescriptionOrFallback(string language, string fallbackName)
    {
        var translation = GetDescription(language, fallbackToSpanish: false);
        return string.IsNullOrEmpty(translation) ? fallbackName : translation;
    }

    /// <summary>
    /// Updates the Spanish translation
    /// </summary>
    public TranslatedString WithSpanish(BaseTranslatedProperties spanish)
    {
        if (spanish == null)
            throw new ArgumentException("Spanish translation cannot be empty", nameof(spanish));

        return new TranslatedString
        {
            Spanish = spanish,
            English = English
        };
    }

    /// <summary>
    /// Updates the English translation
    /// </summary>
    public TranslatedString WithEnglish(BaseTranslatedProperties? english)
    {
        return new TranslatedString
        {
            Spanish = Spanish,
            English = english
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Spanish;
        yield return English;
    }

    public override string ToString() => Spanish.Name;
}