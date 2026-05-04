using DreamSoft.Application.Features.Admin.MenuGroups.GetMenuGroups;
using DreamSoft.Application.Features.Admin.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Admin.MenuGroups.UpdateMenuGroup;

public record UpdateMenuGroupCommand(
    int Id,
    string Name,
    string Description,
    string Icon,
    int SortOrder,
    TranslationsDto Translations,
    bool IsActive) : IRequest<MenuGroupDto>;
