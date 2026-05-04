using DreamSoft.Application.Features.Admin.MenuOptions.GetMenuOptions;
using DreamSoft.Application.Features.Admin.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Admin.MenuOptions.UpdateMenuOption;

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
    bool IsActive) : IRequest<MenuOptionDto>;
