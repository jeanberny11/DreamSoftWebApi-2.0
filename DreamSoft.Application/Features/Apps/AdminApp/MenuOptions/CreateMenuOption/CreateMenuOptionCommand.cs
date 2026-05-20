using DreamSoft.Application.Features.Apps.AdminApp.MenuOptions.GetMenuOptions;
using DreamSoft.Application.Features.Apps.AdminApp.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.MenuOptions.CreateMenuOption;

public record CreateMenuOptionCommand(
    string Code,
    string Name,
    string Description,
    int ModuleId,
    int MenuGroupId,
    string Route,
    string Icon,
    int SortOrder,
    TranslationsDto Translations) : IRequest<MenuOptionDto>;
