namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Pricing;

public record PricingResponse(
    string SolutionCode,
    string SolutionName,
    string SolutionDescription,
    List<Plans> Plans
);

public record Plans(
    string Code,
    string Name,
    string Description,
    int TrialDays,
    int TierLevel,
    List<PlanPrices> Prices,
    List<PlanLimits> Limits
);

public record PlanPrices(
    PricingBillingCycleDto BillingCycle,
    decimal Price
);

public record PlanLimits(
    string LimitKey,
    int LimitValue,
    string Description
);

public record PricingBillingCycleDto(
    string Code,
    string Name,
    string Description,
    int Months
);