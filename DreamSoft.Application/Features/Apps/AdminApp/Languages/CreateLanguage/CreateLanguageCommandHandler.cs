using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.AdminApp.Languages.GetLanguages;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using DreamSoft.Domain.ValueObjects;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Languages.CreateLanguage;

public class CreateLanguageCommandHandler(
    ILanguageRepository languageRepository,
    IRequestLanguageService languageService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateLanguageCommand, LanguageDto>
{
    public async Task<LanguageDto> Handle(
        CreateLanguageCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await languageRepository.AnyAsync(
            l => l.Code == request.Code.ToLower().Trim(), cancellationToken);

        if (exists)
            throw new ConflictException("LanguageCodeAlreadyExists", request.Code);

        var translations = TranslatedString.Create(
            BaseTranslatedProperties.CreateWithName(request.Translations.Spanish.Name),
            request.Translations.English is not null
                ? BaseTranslatedProperties.CreateWithName(request.Translations.English.Name)
                : null);

        var language = Language.Create(request.Code, request.Name, translations, request.IsDefault);

        await languageRepository.AddAsync(language, cancellationToken);
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
