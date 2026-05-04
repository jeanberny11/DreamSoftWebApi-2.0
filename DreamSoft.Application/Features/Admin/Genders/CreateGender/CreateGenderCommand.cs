using DreamSoft.Application.Features.Admin.Genders.GetGenders;
using DreamSoft.Application.Features.Admin.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Admin.Genders.CreateGender;

public record CreateGenderCommand(
    string Code,
    string Name,
    TranslationsDto Translations) : IRequest<GenderDto>;
