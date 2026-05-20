using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.AdminApp.Genders.GetGenders;
using DreamSoft.Domain.Repositories;
using DreamSoft.Domain.ValueObjects;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Genders.UpdateGender;

public class UpdateGenderCommandHandler(
    IGenderRepository genderRepository,
    IRequestLanguageService languageService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateGenderCommand, GenderDto>
{
    public async Task<GenderDto> Handle(
        UpdateGenderCommand request,
        CancellationToken cancellationToken)
    {
        var gender = await genderRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "Gender", request.Id);

        var translations = TranslatedString.Create(
            BaseTranslatedProperties.CreateWithName(request.Translations.Spanish.Name),
            request.Translations.English is not null
                ? BaseTranslatedProperties.CreateWithName(request.Translations.English.Name)
                : null);

        gender.UpdateDetails(request.Name, translations, request.IsActive);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var language = languageService.Resolve();

        return new GenderDto(
            gender.Id,
            gender.Code,
            gender.Translations.GetNameOrFallback(language, gender.Name),
            gender.IsActive);
    }
}
