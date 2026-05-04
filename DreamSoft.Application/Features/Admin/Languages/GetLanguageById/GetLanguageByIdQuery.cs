using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Admin.Languages.GetLanguages;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Admin.Languages.GetLanguageById;

public record GetLanguageByIdQuery(int Id, string? Language = null) : IRequest<LanguageDto>;

public class GetLanguageByIdQueryHandler(
    ILanguageRepository languageRepository,
    IRequestLanguageService languageService)
    : IRequestHandler<GetLanguageByIdQuery, LanguageDto>
{
    public async Task<LanguageDto> Handle(
        GetLanguageByIdQuery request,
        CancellationToken cancellationToken)
    {
        var lang     = languageService.Resolve(request.Language);
        var language = await languageRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "Language", request.Id);

        return new LanguageDto(
            language.Id,
            language.Code,
            language.Translations.GetNameOrFallback(lang, language.Name),
            language.IsDefault,
            language.IsActive);
    }
}
