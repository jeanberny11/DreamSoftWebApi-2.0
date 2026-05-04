using DreamSoft.Application.Features.Admin.MenuOptions.GetMenuOptions;
using DreamSoft.Application.Features.Admin.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Admin.MenuOptions.CreateMenuOption;

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
