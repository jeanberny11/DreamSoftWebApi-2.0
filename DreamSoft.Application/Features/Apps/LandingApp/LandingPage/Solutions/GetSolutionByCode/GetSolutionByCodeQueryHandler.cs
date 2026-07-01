namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Solutions.GetSolutionByCode;

using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Domain.Repositories;
using MediatR;

public class GetSolutionByCodeQueryHandler(ISolutionRepository solutionRepository)
    : IRequestHandler<GetSolutionByCodeQuery, GetSolutionByCodeResponse>
{
    public async Task<GetSolutionByCodeResponse> Handle(
        GetSolutionByCodeQuery request,
        CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";

        var solution = await solutionRepository.GetByCodeWithPlansAsync(request.Code, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "Solution", request.Code);

        var plans = solution.SubscriptionPlans.Select(sp => new SolutionPlan(
            sp.Id,
            sp.Code,
            sp.Translations.GetNameOrFallback(language, sp.Name),
            sp.Translations.GetDescriptionOrFallback(language, sp.Description),
            sp.TrialDays,
            sp.TierLevel,
            [..sp.PlanPrices.Select(pp => new PlanPrice(pp.BillingCycle.Code, pp.BillingCycle.Name, pp.Price))],
            [..sp.PlanLimits.Select(pl => new PlanLimit(pl.LimitKey, (int)pl.LimitValue, pl.Description))],
            [..sp.PlanMenuOptions.Select(pmo => new PlanOption(
                pmo.MenuOption.Id,
                pmo.MenuOption.Code,
                pmo.MenuOption.Translations.GetNameOrFallback(language, pmo.MenuOption.Name),
                pmo.MenuOption.Translations.GetDescriptionOrFallback(language, pmo.MenuOption.Description),
                pmo.MenuOption.Icon,
                pmo.MenuOption.SortOrder,
                pmo.MenuOption.Module.Code,
                pmo.MenuOption.Module.Translations.GetNameOrFallback(language, pmo.MenuOption.Module.Name),
                pmo.MenuOption.MenuGroup.Code,
                pmo.MenuOption.MenuGroup.Translations.GetNameOrFallback(language, pmo.MenuOption.MenuGroup.Name)
            ))]
        )).ToList();

        return new GetSolutionByCodeResponse(
            solution.Id,
            solution.Code,
            solution.Translations.GetNameOrFallback(language, solution.Name),
            solution.Translations.GetDescriptionOrFallback(language, solution.Description),
            solution.Icon,
            plans);
    }
}
