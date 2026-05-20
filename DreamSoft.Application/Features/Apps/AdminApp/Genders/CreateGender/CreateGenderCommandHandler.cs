using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.AdminApp.Genders.GetGenders;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using DreamSoft.Domain.ValueObjects;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Genders.CreateGender;

public class CreateGenderCommandHandler(
    IGenderRepository genderRepository,
    IRequestLanguageService languageService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateGenderCommand, GenderDto>
{
    public async Task<GenderDto> Handle(
        CreateGenderCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await genderRepository.AnyAsync(
            g => g.Code == request.Code.ToUpper().Trim(), cancellationToken);

        if (exists)
            throw new ConflictException("GenderCodeAlreadyExists", request.Code);

        var translations = TranslatedString.Create(
            BaseTranslatedProperties.CreateWithName(request.Translations.Spanish.Name),
            request.Translations.English is not null
                ? BaseTranslatedProperties.CreateWithName(request.Translations.English.Name)
                : null);

        var gender = Gender.Create(request.Code, request.Name, translations);

        await genderRepository.AddAsync(gender, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var language = languageService.Resolve();

        return new GenderDto(
            gender.Id,
            gender.Code,
            gender.Translations.GetNameOrFallback(language, gender.Name),
            gender.IsActive);
    }
}
