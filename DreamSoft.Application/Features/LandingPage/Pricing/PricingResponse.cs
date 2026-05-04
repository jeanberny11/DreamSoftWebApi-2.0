namespace DreamSoft.Application.Features.LandingPage.Pricing;

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
    string BillingCycleCode,
    decimal Price
);

public record PlanLimits(
    string LimitKey,
    int LimitValue,
    string Description
);