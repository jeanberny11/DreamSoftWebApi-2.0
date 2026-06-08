using DreamSoft.Application.Features.Apps.AdminApp.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Languages.UpdateLanguage;

public record UpdateLanguageCommand(
    int Id,
    string Name,
    bool IsDefault,
    TranslationsDto Translations,
    bool IsActive) : IRequest<Unit>;
