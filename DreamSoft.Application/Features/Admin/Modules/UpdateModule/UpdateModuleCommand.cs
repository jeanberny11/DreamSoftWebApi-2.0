using DreamSoft.Application.Features.Admin.Modules.GetModules;
using DreamSoft.Application.Features.Admin.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Admin.Modules.UpdateModule;

public record UpdateModuleCommand(
    int Id,
    string Name,
    string Description,
    string Icon,
    int SortOrder,
    TranslationsDto Translations,
    bool IsActive) : IRequest<ModuleDto>;
