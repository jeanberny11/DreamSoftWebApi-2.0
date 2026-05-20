using DreamSoft.Application.Features.Apps.AdminApp.Modules.GetModules;
using DreamSoft.Application.Features.Apps.AdminApp.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Modules.CreateModule;

public record CreateModuleCommand(
    string Code,
    string Name,
    string Description,
    string Icon,
    int SortOrder,
    TranslationsDto Translations) : IRequest<ModuleDto>;
