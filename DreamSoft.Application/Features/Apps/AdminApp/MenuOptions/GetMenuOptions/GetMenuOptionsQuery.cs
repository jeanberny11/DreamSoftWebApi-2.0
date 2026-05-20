using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.MenuOptions.GetMenuOptions;

// ── Shared DTO ────────────────────────────────────────────────────────────────

public record MenuOptionDto(
    int Id,
    string Code,
    string Name,
    string Description,
    int ModuleId,
    int MenuGroupId,
    string Route,
    string Icon,
    int SortOrder,
    bool IsActive);

// ── Get All (active + inactive) — SuperAdmin only ─────────────────────────────

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
                mo.Id,
                mo.Code,
                mo.Translations.GetNameOrFallback(language, mo.Name),
                mo.Translations.GetDescriptionOrFallback(language, mo.Description),
                mo.ModuleId,
                mo.MenuGroupId,
                mo.Route,
                mo.Icon,
                mo.SortOrder,
                mo.IsActive))
            .ToList();
    }
}

// ── Get All Active — Public ───────────────────────────────────────────────────

public record GetActiveMenuOptionsQuery(string? Language = null) : IRequest<IReadOnlyList<MenuOptionDto>>;

public class GetActiveMenuOptionsQueryHandler(
    IMenuOptionRepository menuOptionRepository,
    IRequestLanguageService languageService)
    : IRequestHandler<GetActiveMenuOptionsQuery, IReadOnlyList<MenuOptionDto>>
{
    public async Task<IReadOnlyList<MenuOptionDto>> Handle(
        GetActiveMenuOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var language    = languageService.Resolve(request.Language);
        var menuOptions = await menuOptionRepository.GetAllActiveAsync(cancellationToken);

        return menuOptions
            .Select(mo => new MenuOptionDto(
                mo.Id,
                mo.Code,
                mo.Translations.GetNameOrFallback(language, mo.Name),
                mo.Translations.GetDescriptionOrFallback(language, mo.Description),
                mo.ModuleId,
                mo.MenuGroupId,
                mo.Route,
                mo.Icon,
                mo.SortOrder,
                mo.IsActive))
            .ToList();
    }
}
