using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Admin.Languages.GetLanguages;
using DreamSoft.Domain.Repositories;
using DreamSoft.Domain.ValueObjects;
using MediatR;

namespace DreamSoft.Application.Features.Admin.Languages.UpdateLanguage;

public class UpdateLanguageCommandHandler(
    ILanguageRepository languageRepository,
    IRequestLanguageService languageService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateLanguageCommand, LanguageDto>
{
    public async Task<LanguageDto> Handle(
        UpdateLanguageCommand request,
        CancellationToken cancellationToken)
    {
        var language = await languageRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "Language", request.Id);

        var translations = TranslatedString.Create(
            BaseTranslatedProperties.CreateWithName(request.Translations.Spanish.Name),
            request.Translations.English is not null
                ? BaseTranslatedProperties.CreateWithName(request.Translations.English.Name)
                : null);

        language.UpdateDetails(request.Name, translations, request.IsDefault, request.IsActive);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var lang = languageService.Resolve();

        return new LanguageDto(
            language.Id,
            language.Code,
            language.Translations.GetNameOrFallback(lang, language.Name),
            language.IsDefault,
            language.IsActive);
    }
}
