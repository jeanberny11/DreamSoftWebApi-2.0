using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Admin.MenuGroups.GetMenuGroups;

// ── Shared DTO ────────────────────────────────────────────────────────────────

public record MenuGroupDto(
    int Id,
    string Code,
    string Name,
    string Description,
    string Icon,
    int SortOrder,
    bool IsActive);

// ── Get All (active + inactive) — SuperAdmin only ─────────────────────────────

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
                mg.Id,
                mg.Code,
                mg.Translations.GetNameOrFallback(language, mg.Name),
                mg.Translations.GetDescriptionOrFallback(language, mg.Description),
                mg.Icon,
                mg.SortOrder,
                mg.IsActive))
            .ToList();
    }
}

// ── Get All Active — Public ───────────────────────────────────────────────────

public record GetActiveMenuGroupsQuery(string? Language = null) : IRequest<IReadOnlyList<MenuGroupDto>>;

public class GetActiveMenuGroupsQueryHandler(
    IMenuGroupRepository menuGroupRepository,
    IRequestLanguageService languageService)
    : IRequestHandler<GetActiveMenuGroupsQuery, IReadOnlyList<MenuGroupDto>>
{
    public async Task<IReadOnlyList<MenuGroupDto>> Handle(
        GetActiveMenuGroupsQuery request,
        CancellationToken cancellationToken)
    {
        var language   = languageService.Resolve(request.Language);
        var menuGroups = await menuGroupRepository.GetAllActiveAsync(cancellationToken);

        return menuGroups
            .Select(mg => new MenuGroupDto(
                mg.Id,
                mg.Code,
                mg.Translations.GetNameOrFallback(language, mg.Name),
                mg.Translations.GetDescriptionOrFallback(language, mg.Description),
                mg.Icon,
                mg.SortOrder,
                mg.IsActive))
            .ToList();
    }
}
