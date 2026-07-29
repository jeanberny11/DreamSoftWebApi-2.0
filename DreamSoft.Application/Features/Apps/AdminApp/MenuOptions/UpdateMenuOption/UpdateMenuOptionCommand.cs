using DreamSoft.Application.Features.Apps.AdminApp.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.MenuOptions.UpdateMenuOption;

public record UpdateMenuOptionCommand(
    int Id,
    string Name,
    string Description,
    int ModuleId,
    int MenuGroupId,
    string Route,
    string Icon,
    int SortOrder,
    TranslationsDto Translations,
    bool IsActive) : IRequest<Unit>;
