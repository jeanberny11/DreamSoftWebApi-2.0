using DreamSoft.Application.Features.Apps.LandingApp.Subscription.GetSubscriptions;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Subscription.GetSubscriptionById;

/// <summary>
/// Returns a single subscription belonging to the authenticated tenant,
/// regardless of status (including CANCELLED — unlike the list query,
/// which excludes cancelled subscriptions). Used by the Manage page.
/// </summary>
public record GetSubscriptionByIdQuery(
    int SubscriptionId,
    string? Language = null
) : IRequest<TenantSubscriptionDto>;
