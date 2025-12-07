namespace DreamSoft.Domain.ValueObjects;

public class TranslatedString : ValueObject
{
    public string Spanish { get; private set; } = string.Empty;
    public string? English { get; private set; }

    // Private constructor for EF Core
    private TranslatedString()
    {
    }

    // Factory method for creating instances
    public static TranslatedString Create(string spanish, string? english = null)
    {
        if (string.IsNullOrWhiteSpace(spanish))
            throw new ArgumentException("Spanish translation is required", nameof(spanish));

        return new TranslatedString
        {
            Spanish = spanish.Trim(),
            English = english?.Trim()
        };
    }

    /// <summary>
    /// Gets the translation in the specified language
    /// </summary>
    /// <param name="language">Language code (es, en)</param>
    /// <param name="fallbackToSpanish">If true, returns Spanish when English is not available</param>
    public string Get(string language = "es", bool fallbackToSpanish = true)
    {
        return language?.ToLower() switch
        {
            "en" when !string.IsNullOrEmpty(English) => English,
            "en" when fallbackToSpanish => Spanish,
            "en" => string.Empty, // Return empty if no fallback
            _ => Spanish
        };
    }

    /// <summary>
    /// Gets translation with name as ultimate fallback
    /// </summary>
    /// <param name="language">Language code (es, en)</param>
    /// <param name="fallbackName">Fallback name to use if translation is not available</param>
    /// <returns>Translation or fallback name</returns>
    public string GetOrFallback(string language, string fallbackName)
    {
        var translation = Get(language, fallbackToSpanish: false);
        return string.IsNullOrEmpty(translation) ? fallbackName : translation;
    }

    /// <summary>
    /// Updates the Spanish translation
    /// </summary>
    public TranslatedString WithSpanish(string spanish)
    {
        if (string.IsNullOrWhiteSpace(spanish))
            throw new ArgumentException("Spanish translation cannot be empty", nameof(spanish));

        return new TranslatedString
        {
            Spanish = spanish.Trim(),
            English = English
        };
    }

    /// <summary>
    /// Updates the English translation
    /// </summary>
    public TranslatedString WithEnglish(string? english)
    {
        return new TranslatedString
        {
            Spanish = Spanish,
            English = english?.Trim()
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Spanish;
        yield return English;
    }

    public override string ToString() => Spanish;
}