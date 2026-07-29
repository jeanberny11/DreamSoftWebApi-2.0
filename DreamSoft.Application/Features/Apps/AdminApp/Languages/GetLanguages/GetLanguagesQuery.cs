using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.AdminApp.Languages.DTOs;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Languages.GetLanguages;

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
                l.Id, l.Code,
                l.Translations.GetNameOrFallback(lang, l.Name),
                l.IsDefault, l.IsActive))
            .ToList();
    }
}
