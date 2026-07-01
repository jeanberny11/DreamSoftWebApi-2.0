using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Subscription.GetSubscriptions;

public record GetSubscriptionsQuery(string? Language = null)
    : IRequest<IReadOnlyList<TenantSubscriptionDto>>;

public record TenantSubscriptionDto(
    int Id,
    int SolutionId,
    string SolutionCode,
    string SolutionName,
    string SolutionIcon,
    string? Subdomain,
    int SubscriptionPlanId,
    string SubscriptionPlanCode,
    string SubscriptionPlanName,
    string SubscriptionPlanDescription,
    int TierLevel,
    string BillingCycleCode,
    string BillingCycleName,
    string BillingCycleDescription,
    decimal Price,
    int StatusId,
    string StatusCode,
    string StatusName,
    DateTime StartDate,
    DateTime? EndDate,
    DateTime? TrialEndDate,
    DateTime? CancellationScheduledAt,
    string? Notes
);
