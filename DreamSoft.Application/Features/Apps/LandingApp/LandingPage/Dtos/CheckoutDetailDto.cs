namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Dtos;

// Lightweight solution+plan detail for the "Add New Subscription" checkout
// review page. Resolved from a single PlanId (see CheckoutDetailQuery) —
// carries ALL of the plan's prices (not just one), so the frontend can pick
// the specific PlanPriceId it already has from the picker step without a
// second round trip.
public record CheckoutDetailDto(
    int SolutionId,
    string SolutionCode,
    string SolutionName,
    string SolutionIcon,
    int PlanId,
    string PlanCode,
    string PlanName,
    string PlanDescription,
    int TrialDays,
    List<PlanLimitDto> Limits,
    List<PlanPriceDto> Prices
);
