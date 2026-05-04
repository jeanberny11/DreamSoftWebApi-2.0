using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Admin.MenuOptions.GetMenuOptions;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Admin.MenuOptions.GetMenuOptionById;

public record GetMenuOptionByIdQuery(int Id, string? Language = null) : IRequest<MenuOptionDto>;

public class GetMenuOptionByIdQueryHandler(
    IMenuOptionRepository menuOptionRepository,
    IRequestLanguageService languageService)
    : IRequestHandler<GetMenuOptionByIdQuery, MenuOptionDto>
{
    public async Task<MenuOptionDto> Handle(
        GetMenuOptionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var language   = languageService.Resolve(request.Language);
        var menuOption = await menuOptionRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "MenuOption", request.Id);

        return new MenuOptionDto(
            menuOption.Id,
            menuOption.Code,
            menuOption.Translations.GetNameOrFallback(language, menuOption.Name),
            menuOption.Translations.GetDescriptionOrFallback(language, menuOption.Description),
            menuOption.ModuleId,
            menuOption.MenuGroupId,
            menuOption.Route,
            menuOption.Icon,
            menuOption.SortOrder,
            menuOption.IsActive);
    }
}
