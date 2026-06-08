namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Solutions.GetSolutionFeatures;

using DreamSoft.Domain.Repositories;
using MediatR;

public class SolutionQueryHandler(ISolutionRepository solutionRepository)
    : IRequestHandler<SolutionQuery, List<SolutionResponse>>
{
    public async Task<List<SolutionResponse>> Handle(
        SolutionQuery request,
        CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";

        var solutions = await solutionRepository.GetAllActiveWithModulesAsync(cancellationToken);

        return [.. solutions.Select(solution => new SolutionResponse(
            solution.Code,
            solution.Translations.GetNameOrFallback(language, solution.Name),
            solution.Translations.GetDescriptionOrFallback(language, solution.Description),
            solution.Icon,
            [.. solution.SubscriptionPlans
                .Where(sp => sp.IsActive)
                .SelectMany(sp => sp.PlanMenuOptions)
                .Select(pmo => pmo.MenuOption)
                .Where(mo => mo.IsActive)
                .Select(mo => mo.Module)
                .Where(m => m.IsActive)
                .DistinctBy(m => m.Id)
                .OrderBy(m => m.SortOrder)
                .Select(m => new SolutionFeature(
                    m.Code,
                    m.Translations.GetNameOrFallback(language, m.Name),
                    m.Translations.GetDescriptionOrFallback(language, m.Description),
                    m.Icon))]))];
    }
}
