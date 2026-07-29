using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Subscription.GetChangePlanPreview;

/// <summary>
/// Previews the financial impact of switching to a new plan/price before
/// the tenant commits, using the exact same eligibility rules as
/// ChangePlanCommand so a successful preview never leads to a rejected
/// confirm. Returns everything the review page needs in one call.
/// </summary>
public record GetChangePlanPreviewQuery(
    int NewPlanPriceId,
    bool ProrationImmediate,
    string? Language = null
) : IRequest<ChangePlanPreviewResponse>;

public record ChangePlanPreviewResponse(
    // Current plan (left side of the comparison)
    string CurrentPlanName,
    decimal CurrentPrice,
    string CurrentBillingCycleName,

    // Solution + new plan (right side)
    string SolutionName,
    string SolutionIcon,
    string NewPlanName,
    decimal NewPrice,
    string NewBillingCycleName,
    int NewPlanPriceId,

    // Proration breakdown
    decimal CreditAmount,
    decimal ChargeAmount,
    decimal AmountDueNow,
    string Currency,
    DateTime NextBillingDate,
    decimal NextBillingAmount,
    DateTime ProrationDate,
    bool ProrationImmediate,

    // Trial-preservation messaging
    bool HasActiveTrial,
    DateTime? TrialEndDate
);
