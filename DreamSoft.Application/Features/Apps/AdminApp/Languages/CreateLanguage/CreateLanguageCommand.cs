using DreamSoft.Application.Features.Apps.AdminApp.Languages.DTOs;
using DreamSoft.Application.Features.Apps.AdminApp.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Languages.CreateLanguage;

public record CreateLanguageCommand(
    string Code,
    string Name,
    bool IsDefault,
    TranslationsDto Translations) : IRequest<LanguageDto>;
