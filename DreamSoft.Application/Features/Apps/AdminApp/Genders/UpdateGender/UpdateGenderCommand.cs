using DreamSoft.Application.Features.Apps.AdminApp.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Genders.UpdateGender;

public record UpdateGenderCommand(
    int Id,
    string Name,
    TranslationsDto Translations,
    bool IsActive) : IRequest<Unit>;
