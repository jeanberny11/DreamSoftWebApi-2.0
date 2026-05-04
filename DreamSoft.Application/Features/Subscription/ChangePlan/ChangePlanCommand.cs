using MediatR;

namespace DreamSoft.Application.Features.Subscription.ChangePlan;

/// <summary>
/// Changes the tenant's current active subscription to a new plan/billing cycle.
/// NewPlanPriceId identifies the target plan + billing cycle.
/// ProrationImmediate controls whether the price difference is billed right away
/// (true) or applied at the next billing cycle (false).
/// </summary>
public record ChangePlanCommand(
    int NewPlanPriceId,
    bool ProrationImmediate = true
) : IRequest<ChangePlanResponse>;

public record ChangePlanResponse(string Message);
