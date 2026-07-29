using DreamSoft.Application.Features.Apps.AdminApp.Genders.DTOs;
using DreamSoft.Application.Features.Apps.AdminApp.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Genders.CreateGender;

public record CreateGenderCommand(
    string Code,
    string Name,
    TranslationsDto Translations) : IRequest<GenderDto>;
