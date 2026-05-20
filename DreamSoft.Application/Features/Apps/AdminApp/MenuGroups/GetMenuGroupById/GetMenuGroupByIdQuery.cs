using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.AdminApp.MenuGroups.GetMenuGroups;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.MenuGroups.GetMenuGroupById;

public record GetMenuGroupByIdQuery(int Id, string? Language = null) : IRequest<MenuGroupDto>;

public class GetMenuGroupByIdQueryHandler(
    IMenuGroupRepository menuGroupRepository,
    IRequestLanguageService languageService)
    : IRequestHandler<GetMenuGroupByIdQuery, MenuGroupDto>
{
    public async Task<MenuGroupDto> Handle(
        GetMenuGroupByIdQuery request,
        CancellationToken cancellationToken)
    {
        var language  = languageService.Resolve(request.Language);
        var menuGroup = await menuGroupRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "MenuGroup", request.Id);

        return new MenuGroupDto(
            menuGroup.Id,
            menuGroup.Code,
            menuGroup.Translations.GetNameOrFallback(language, menuGroup.Name),
            menuGroup.Translations.GetDescriptionOrFallback(language, menuGroup.Description),
            menuGroup.Icon,
            menuGroup.SortOrder,
            menuGroup.IsActive);
    }
}
