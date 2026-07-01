namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Solutions.GetSolutionByCode;

using MediatR;

public record GetSolutionByCodeQuery(string Code, string? Language = null)
    : IRequest<GetSolutionByCodeResponse>;

public record GetSolutionByCodeResponse(
    int SolutionId,
    string Code,
    string Name,
    string Description,
    string IconUrl,
    List<SolutionPlan> Plans);

public record SolutionPlan(
    int PlanId,
    string Code,
    string Name,
    string Description,
    int TrialDays,
    int TierLevel,
    List<PlanPrice> Prices,
    List<PlanLimit> Limits,
    List<PlanOption> Options
);

public record PlanPrice(
    string BillingCycleCode,
    string BillingCycleName,
    decimal Price
);

public record PlanLimit(
    string LimitKey,
    int LimitValue,
    string Description
);

public record PlanOption(
    int OptionId,
    string Code,
    string Name,
    string Description,
    string IconUrl,
    int SortOrder,
    string ModuleCode,
    string ModuleName,
    string GroupCode,
    string GroupName
);