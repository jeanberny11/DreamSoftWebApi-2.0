using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Pricing;

public class PricingQueryHandler(
    ISolutionRepository solutionRepository,
    ISubscriptionPlanRepository subscriptionPlanRepository,
    IPlanPriceRepository planPriceRepository,
    IPlanLimitRepository planLimitRepository)
    : IRequestHandler<PricingQuery, List<PricingResponse>>
{
    public async Task<List<PricingResponse>> Handle(
        PricingQuery request,
        CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";
        var solutions = await solutionRepository.GetAllActiveAsync(cancellationToken);
        var result = new List<PricingResponse>();

        foreach (var solution in solutions)
        {
            var plans = await subscriptionPlanRepository.GetBySolutionIdAsync(solution.Id, cancellationToken);
            var planDtos = new List<Plans>();

            foreach (var plan in plans)
            {
                var prices = await planPriceRepository.GetByPlanIdAsync(plan.Id, cancellationToken);
                var limits = await planLimitRepository.GetByPlanIdAsync(plan.Id, cancellationToken);

                planDtos.Add(new Plans(
                    plan.Code,
                    plan.GetTranslatedName(language),
                    plan.Translations.GetDescriptionOrFallback(language, plan.Description),
                    plan.TrialDays,
                    plan.TierLevel,
                    [.. prices.Select(p => new PlanPrices(p.BillingCycle?.Code ?? string.Empty, p.Price))],
                    [.. limits.Select(l => new PlanLimits(l.LimitKey, (int)l.LimitValue, l.Description))]
                ));
            }

            result.Add(new PricingResponse(
                solution.Code,
                solution.GetTranslatedName(language),
                solution.Translations.GetDescriptionOrFallback(language, solution.Description),
                planDtos
            ));
        }

        return result;
    }
}
