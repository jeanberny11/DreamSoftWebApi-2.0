using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.AdminApp.MenuOptions.DTOs;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.MenuOptions.GetMenuOptions;

public record GetMenuOptionsQuery(string? Language = null) : IRequest<IReadOnlyList<MenuOptionDto>>;

public class GetMenuOptionsQueryHandler(
    IMenuOptionRepository menuOptionRepository,
    IRequestLanguageService languageService)
    : IRequestHandler<GetMenuOptionsQuery, IReadOnlyList<MenuOptionDto>>
{
    public async Task<IReadOnlyList<MenuOptionDto>> Handle(
        GetMenuOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var language    = languageService.Resolve(request.Language);
        var menuOptions = await menuOptionRepository.GetAllAsync(cancellationToken);

        return menuOptions
            .Select(mo => new MenuOptionDto(
                mo.Id, mo.Code,
                mo.Translations.GetNameOrFallback(language, mo.Name),
                mo.Translations.GetDescriptionOrFallback(language, mo.Description),
                mo.ModuleId, mo.MenuGroupId, mo.Route, mo.Icon, mo.SortOrder, mo.IsActive))
            .ToList();
    }
}
