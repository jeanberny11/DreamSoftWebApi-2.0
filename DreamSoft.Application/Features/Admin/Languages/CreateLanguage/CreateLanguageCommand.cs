using DreamSoft.Application.Features.Admin.Languages.GetLanguages;
using DreamSoft.Application.Features.Admin.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Admin.Languages.CreateLanguage;

public record CreateLanguageCommand(
    string Code,
    string Name,
    bool IsDefault,
    TranslationsDto Translations) : IRequest<LanguageDto>;
