using DreamSoft.Application.Features.Admin.Languages.GetLanguages;
using DreamSoft.Application.Features.Admin.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Admin.Languages.UpdateLanguage;

public record UpdateLanguageCommand(
    int Id,
    string Name,
    bool IsDefault,
    TranslationsDto Translations,
    bool IsActive) : IRequest<LanguageDto>;
