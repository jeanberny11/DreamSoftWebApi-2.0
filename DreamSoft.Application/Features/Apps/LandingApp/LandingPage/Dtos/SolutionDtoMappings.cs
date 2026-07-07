using DreamSoft.Domain.Entities;

namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Dtos;

public static class SolutionDtoMappings
{
    public static SolutionDto ToDto(this Solution solution, string language)
        => new(
            SolutionId: solution.Id,
            Code: solution.Code,
            Name: solution.GetTranslatedName(language),
            Description: solution.Translations.GetDescriptionOrFallback(language, solution.Description),
            Icon: solution.Icon,
            Plans: [.. solution.SubscriptionPlans
                .Select(plan => new SubscriptionPlanDto(
                    PlanId: plan.Id,
                    Code: plan.Code,
                    Name: plan.GetTranslatedName(language),
                    Description: plan.Translations.GetDescriptionOrFallback(language, plan.Description),
                    Limits: [.. plan.PlanLimits
                        .Select(limit => new PlanLimitDto(
                            PlanLimitId: limit.Id,
                            LimitKey: limit.LimitKey,
                            LimitValue: (int)limit.LimitValue,
                            Description: limit.Description))],
                    Prices: [.. plan.PlanPrices
                        .Select(price => new PlanPriceDto(
                            PlanPriceId: price.Id,
                            Price: price.Price,
                            BillingCycle: new BillingCycleDto(
                                BillingCycleId: price.BillingCycle.Id,
                                Code: price.BillingCycle.Code,
                                Name: price.BillingCycle.Translations.GetNameOrFallback(language, price.BillingCycle.Name),
                                Description: price.BillingCycle.Translations.GetDescriptionOrFallback(language, price.BillingCycle.Description),
                                Months: price.BillingCycle.Months)))],
                    Features: [.. plan.PlanMenuOptions
                        .Select(pmo => pmo.MenuOption)
                        .Where(mo => mo.IsActive && mo.Module.IsActive)
                        .DistinctBy(mo => mo.Id)
                        .GroupBy(mo => mo.Module)
                        .OrderBy(g => g.Key.SortOrder)
                        .Select(g => g.Key.ToDto(g, language))],
                    TrialDays: plan.TrialDays,
                    TierLevel: plan.TierLevel))]);
}
