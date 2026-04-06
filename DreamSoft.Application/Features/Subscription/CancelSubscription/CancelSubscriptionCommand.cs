using MediatR;

namespace DreamSoft.Application.Features.Subscription.CancelSubscription;

/// <summary>
/// Cancels the tenant's active subscription for a given solution.
/// CancelImmediately = true  → subscription ends now.
/// CancelImmediately = false → subscription remains active until the end of the
///                             current billing period (recommended default).
/// </summary>
public record CancelSubscriptionCommand(
    int SolutionId,
    bool CancelImmediately = false
) : IRequest<CancelSubscriptionResponse>;

public record CancelSubscriptionResponse(string Message);
