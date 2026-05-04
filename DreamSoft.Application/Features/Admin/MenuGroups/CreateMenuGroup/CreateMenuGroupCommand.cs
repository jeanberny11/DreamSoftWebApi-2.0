using DreamSoft.Application.Features.Admin.MenuGroups.GetMenuGroups;
using DreamSoft.Application.Features.Admin.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Admin.MenuGroups.CreateMenuGroup;

public record CreateMenuGroupCommand(
    string Code,
    string Name,
    string Description,
    string Icon,
    int SortOrder,
    TranslationsDto Translations) : IRequest<MenuGroupDto>;
