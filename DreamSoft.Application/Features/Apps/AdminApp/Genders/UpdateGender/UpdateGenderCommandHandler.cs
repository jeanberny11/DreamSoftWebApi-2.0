using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Domain.Repositories;
using DreamSoft.Domain.ValueObjects;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Genders.UpdateGender;

public class UpdateGenderCommandHandler(
    IGenderRepository genderRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateGenderCommand, Unit>
{
    public async Task<Unit> Handle(
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

        return Unit.Value;
    }
}
