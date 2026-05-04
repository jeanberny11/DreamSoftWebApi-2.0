using DreamSoft.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace DreamSoft.Infrastructure.Services.Common;

public class RequestLanguageService(IHttpContextAccessor httpContextAccessor)
    : IRequestLanguageService
{
    private const string DefaultLanguage = "es";

    private static readonly HashSet<string> SupportedLanguages =
        new(StringComparer.OrdinalIgnoreCase) { "es", "en" };

    public string Resolve(string? explicitLanguage = null)
    {
        // 1. Explicit query param takes priority
        if (!string.IsNullOrWhiteSpace(explicitLanguage))
        {
            var normalized = explicitLanguage.Trim().ToLowerInvariant();
            return SupportedLanguages.Contains(normalized) ? normalized : DefaultLanguage;
        }

        // 2. Read Accept-Language header
        var acceptLanguage = httpContextAccessor.HttpContext?
            .Request.Headers["Accept-Language"].ToString();

        if (!string.IsNullOrWhiteSpace(acceptLanguage))
        {
            // Header can be "en-US,en;q=0.9,es;q=0.8" — take the first language tag
            var primary = acceptLanguage
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Split(';')[0].Trim().ToLowerInvariant())
                .FirstOrDefault();

            if (primary is not null)
            {
                // Normalize region variants: "en-us" → "en"
                var languageCode = primary.Contains('-')
                    ? primary.Split('-')[0]
                    : primary;

                if (SupportedLanguages.Contains(languageCode))
                    return languageCode;
            }
        }

        // 3. Default fallback
        return DefaultLanguage;
    }
}
