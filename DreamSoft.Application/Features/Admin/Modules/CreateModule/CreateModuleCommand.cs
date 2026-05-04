using DreamSoft.Application.Features.Admin.Modules.GetModules;
using DreamSoft.Application.Features.Admin.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Admin.Modules.CreateModule;

public record CreateModuleCommand(
    string Code,
    string Name,
    string Description,
    string Icon,
    int SortOrder,
    TranslationsDto Translations) : IRequest<ModuleDto>;
