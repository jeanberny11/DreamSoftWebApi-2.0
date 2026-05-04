namespace DreamSoft.Application.Common.Interfaces;

/// <summary>
/// Resolves the preferred language for the current request.
/// Priority: explicit query param → Accept-Language header → default fallback ("es").
/// </summary>
public interface IRequestLanguageService
{
    /// <summary>
    /// Returns the resolved language code (e.g. "es", "en").
    /// If an explicit language is provided it takes priority.
    /// Otherwise reads the Accept-Language header from the current HTTP request.
    /// Falls back to "es" if nothing is resolvable.
    /// </summary>
    string Resolve(string? explicitLanguage = null);
}
