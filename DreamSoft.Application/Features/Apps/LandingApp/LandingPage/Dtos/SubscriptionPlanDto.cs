namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Dtos;

public record SubscriptionPlanDto(
    int PlanId,
    string Code,
    string Name,
    string Description,
    List<PlanLimitDto> Limits,
    List<PlanPriceDto> Prices,
    List<ModuleDto> Features,
    int TrialDays,
    int TierLevel
);