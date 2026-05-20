using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.AdminApp.Genders.GetGenders;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Genders.GetGenderById;

public record GetGenderByIdQuery(int Id, string? Language = null) : IRequest<GenderDto>;

public class GetGenderByIdQueryHandler(
    IGenderRepository genderRepository,
    IRequestLanguageService languageService)
    : IRequestHandler<GetGenderByIdQuery, GenderDto>
{
    public async Task<GenderDto> Handle(
        GetGenderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var language = languageService.Resolve(request.Language);
        var gender   = await genderRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "Gender", request.Id);

        return new GenderDto(
            gender.Id,
            gender.Code,
            gender.Translations.GetNameOrFallback(language, gender.Name),
            gender.IsActive);
    }
}
