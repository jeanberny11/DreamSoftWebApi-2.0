using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Languages.GetLanguages;

// ── Shared DTO ────────────────────────────────────────────────────────────────

public record LanguageDto(int Id, string Code, string Name, bool IsDefault, bool IsActive);

// ── Get All (active + inactive) — SuperAdmin only ─────────────────────────────

public record GetLanguagesQuery(string? Language = null) : IRequest<IReadOnlyList<LanguageDto>>;

public class GetLanguagesQueryHandler(
    ILanguageRepository languageRepository,
    IRequestLanguageService languageService)
    : IRequestHandler<GetLanguagesQuery, IReadOnlyList<LanguageDto>>
{
    public async Task<IReadOnlyList<LanguageDto>> Handle(
        GetLanguagesQuery request,
        CancellationToken cancellationToken)
    {
        var lang      = languageService.Resolve(request.Language);
        var languages = await languageRepository.GetAllAsync(cancellationToken);

        return languages
            .Select(l => new LanguageDto(
                l.Id,
                l.Code,
                l.Translations.GetNameOrFallback(lang, l.Name),
                l.IsDefault,
                l.IsActive))
            .ToList();
    }
}

// ── Get All Active — Public ───────────────────────────────────────────────────

public record GetActiveLanguagesQuery(string? Language = null) : IRequest<IReadOnlyList<LanguageDto>>;

public class GetActiveLanguagesQueryHandler(
    ILanguageRepository languageRepository,
    IRequestLanguageService languageService)
    : IRequestHandler<GetActiveLanguagesQuery, IReadOnlyList<LanguageDto>>
{
    public async Task<IReadOnlyList<LanguageDto>> Handle(
        GetActiveLanguagesQuery request,
        CancellationToken cancellationToken)
    {
        var lang      = languageService.Resolve(request.Language);
        var languages = await languageRepository.GetAllActiveAsync(cancellationToken);

        return languages
            .Select(l => new LanguageDto(
                l.Id,
                l.Code,
                l.Translations.GetNameOrFallback(lang, l.Name),
                l.IsDefault,
                l.IsActive))
            .ToList();
    }
}
