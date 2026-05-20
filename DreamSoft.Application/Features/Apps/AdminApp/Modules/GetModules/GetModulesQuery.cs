using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Modules.GetModules;

// ── Shared DTO ────────────────────────────────────────────────────────────────

public record ModuleDto(
    int Id,
    string Code,
    string Name,
    string Description,
    string Icon,
    int SortOrder,
    bool IsActive);

// ── Get All (active + inactive) — SuperAdmin only ─────────────────────────────

public record GetModulesQuery(string? Language = null) : IRequest<IReadOnlyList<ModuleDto>>;

public class GetModulesQueryHandler(
    IModuleRepository moduleRepository,
    IRequestLanguageService languageService)
    : IRequestHandler<GetModulesQuery, IReadOnlyList<ModuleDto>>
{
    public async Task<IReadOnlyList<ModuleDto>> Handle(
        GetModulesQuery request,
        CancellationToken cancellationToken)
    {
        var language = languageService.Resolve(request.Language);
        var modules  = await moduleRepository.GetAllAsync(cancellationToken);

        return modules
            .Select(m => new ModuleDto(
                m.Id,
                m.Code,
                m.Translations.GetNameOrFallback(language, m.Name),
                m.Translations.GetDescriptionOrFallback(language, m.Description),
                m.Icon,
                m.SortOrder,
                m.IsActive))
            .ToList();
    }
}

// ── Get All Active — Public ───────────────────────────────────────────────────

public record GetActiveModulesQuery(string? Language = null) : IRequest<IReadOnlyList<ModuleDto>>;

public class GetActiveModulesQueryHandler(
    IModuleRepository moduleRepository,
    IRequestLanguageService languageService)
    : IRequestHandler<GetActiveModulesQuery, IReadOnlyList<ModuleDto>>
{
    public async Task<IReadOnlyList<ModuleDto>> Handle(
        GetActiveModulesQuery request,
        CancellationToken cancellationToken)
    {
        var language = languageService.Resolve(request.Language);
        var modules  = await moduleRepository.GetAllActiveAsync(cancellationToken);

        return modules
            .Select(m => new ModuleDto(
                m.Id,
                m.Code,
                m.Translations.GetNameOrFallback(language, m.Name),
                m.Translations.GetDescriptionOrFallback(language, m.Description),
                m.Icon,
                m.SortOrder,
                m.IsActive))
            .ToList();
    }
}
