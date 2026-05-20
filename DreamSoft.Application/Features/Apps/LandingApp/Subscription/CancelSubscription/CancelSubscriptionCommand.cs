using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Subscription.CancelSubscription;

/// <summary>
/// Cancels the tenant's active subscription for a given solution.
/// CancelImmediately = true  → subscription ends now.
/// CancelImmediately = false → subscription remains active until the end of the
///                             current billing period (recommended default).
/// CancellationReason: optional short code from the frontend (e.g. "too_expensive").
/// CancellationFeedback: optional free-text comment from the tenant.
/// </summary>
public record CancelSubscriptionCommand(
    int SolutionId,
    bool CancelImmediately = false,
    string? CancellationReason = null,
    string? CancellationFeedback = null
) : IRequest<CancelSubscriptionResponse>;

public record CancelSubscriptionResponse(
    string Message,
    DateTime? ScheduledEndDate = null
);
