using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Admin.Solutions.GetSolutions;

// ── Shared DTO ────────────────────────────────────────────────────────────────

public record SolutionDto(
    int     Id,
    string  Code,
    string  Name,
    string? Description,
    string? Icon,
    int     SortOrder,
    bool    IsActive);

// ── Get All (active + inactive) — SuperAdmin only ─────────────────────────────

public record GetSolutionsQuery(string? Language = null) : IRequest<IReadOnlyList<SolutionDto>>;

public class GetSolutionsQueryHandler(
    ISolutionRepository solutionRepository,
    IRequestLanguageService languageService)
    : IRequestHandler<GetSolutionsQuery, IReadOnlyList<SolutionDto>>
{
    public async Task<IReadOnlyList<SolutionDto>> Handle(
        GetSolutionsQuery request,
        CancellationToken cancellationToken)
    {
        var language  = languageService.Resolve(request.Language);
        var solutions = await solutionRepository.GetAllAsync(cancellationToken);

        return solutions
            .Select(s => new SolutionDto(
                s.Id,
                s.Code,
                s.Translations.GetNameOrFallback(language, s.Name),
                s.Translations.GetDescriptionOrFallback(language, s.Description),
                s.Icon,
                s.SortOrder,
                s.IsActive))
            .ToList();
    }
}

// ── Get All Active — Public ───────────────────────────────────────────────────

public record GetActiveSolutionsQuery(string? Language = null) : IRequest<IReadOnlyList<SolutionDto>>;

public class GetActiveSolutionsQueryHandler(
    ISolutionRepository solutionRepository,
    IRequestLanguageService languageService)
    : IRequestHandler<GetActiveSolutionsQuery, IReadOnlyList<SolutionDto>>
{
    public async Task<IReadOnlyList<SolutionDto>> Handle(
        GetActiveSolutionsQuery request,
        CancellationToken cancellationToken)
    {
        var language  = languageService.Resolve(request.Language);
        var solutions = await solutionRepository.GetAllActiveAsync(cancellationToken);

        return solutions
            .Select(s => new SolutionDto(
                s.Id,
                s.Code,
                s.Translations.GetNameOrFallback(language, s.Name),
                s.Translations.GetDescriptionOrFallback(language, s.Description),
                s.Icon,
                s.SortOrder,
                s.IsActive))
            .ToList();
    }
}
