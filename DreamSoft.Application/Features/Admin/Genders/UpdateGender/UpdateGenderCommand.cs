using DreamSoft.Application.Features.Admin.Genders.GetGenders;
using DreamSoft.Application.Features.Admin.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Admin.Genders.UpdateGender;

public record UpdateGenderCommand(
    int Id,
    string Name,
    TranslationsDto Translations,
    bool IsActive) : IRequest<GenderDto>;
