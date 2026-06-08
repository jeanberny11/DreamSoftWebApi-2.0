using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Domain.Repositories;
using DreamSoft.Domain.ValueObjects;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Languages.UpdateLanguage;

public class UpdateLanguageCommandHandler(
    ILanguageRepository languageRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateLanguageCommand, Unit>
{
    public async Task<Unit> Handle(
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

        return Unit.Value;
    }
}
