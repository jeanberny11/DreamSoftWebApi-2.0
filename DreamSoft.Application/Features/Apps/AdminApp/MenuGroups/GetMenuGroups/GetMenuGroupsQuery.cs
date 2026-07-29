using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.AdminApp.MenuGroups.DTOs;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.MenuGroups.GetMenuGroups;

public record GetMenuGroupsQuery(string? Language = null) : IRequest<IReadOnlyList<MenuGroupDto>>;

public class GetMenuGroupsQueryHandler(
    IMenuGroupRepository menuGroupRepository,
    IRequestLanguageService languageService)
    : IRequestHandler<GetMenuGroupsQuery, IReadOnlyList<MenuGroupDto>>
{
    public async Task<IReadOnlyList<MenuGroupDto>> Handle(
        GetMenuGroupsQuery request,
        CancellationToken cancellationToken)
    {
        var language   = languageService.Resolve(request.Language);
        var menuGroups = await menuGroupRepository.GetAllAsync(cancellationToken);

        return menuGroups
            .Select(mg => new MenuGroupDto(
                mg.Id, mg.Code,
                mg.Translations.GetNameOrFallback(language, mg.Name),
                mg.Translations.GetDescriptionOrFallback(language, mg.Description),
                mg.Icon, mg.SortOrder, mg.IsActive))
            .ToList();
    }
}
