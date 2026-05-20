using DreamSoft.Application.Features.Apps.AdminApp.MenuGroups.GetMenuGroups;
using DreamSoft.Application.Features.Apps.AdminApp.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.MenuGroups.UpdateMenuGroup;

public record UpdateMenuGroupCommand(
    int Id,
    string Name,
    string Description,
    string Icon,
    int SortOrder,
    TranslationsDto Translations,
    bool IsActive) : IRequest<MenuGroupDto>;
