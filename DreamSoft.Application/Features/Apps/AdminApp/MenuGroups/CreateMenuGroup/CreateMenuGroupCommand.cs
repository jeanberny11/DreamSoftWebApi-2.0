using DreamSoft.Application.Features.Apps.AdminApp.MenuGroups.DTOs;
using DreamSoft.Application.Features.Apps.AdminApp.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.MenuGroups.CreateMenuGroup;

public record CreateMenuGroupCommand(
    string Code,
    string Name,
    string Description,
    string Icon,
    int SortOrder,
    TranslationsDto Translations) : IRequest<MenuGroupDto>;
