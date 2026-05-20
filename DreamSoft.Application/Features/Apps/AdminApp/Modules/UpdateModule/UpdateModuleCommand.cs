using DreamSoft.Application.Features.Apps.AdminApp.Modules.GetModules;
using DreamSoft.Application.Features.Apps.AdminApp.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Modules.UpdateModule;

public record UpdateModuleCommand(
    int Id,
    string Name,
    string Description,
    string Icon,
    int SortOrder,
    TranslationsDto Translations,
    bool IsActive) : IRequest<ModuleDto>;
