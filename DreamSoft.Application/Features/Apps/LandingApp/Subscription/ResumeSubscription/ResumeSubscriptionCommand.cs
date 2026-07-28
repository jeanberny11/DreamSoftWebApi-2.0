using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Subscription.ResumeSubscription;

/// <summary>
/// Reverts a pending period-end cancellation for the tenant's subscription —
/// the tenant chose to keep it. Only valid while CancellationScheduledAt is
/// set; the subscription keeps renewing normally afterwards.
/// </summary>
public record ResumeSubscriptionCommand(int SubscriptionId)
    : IRequest<ResumeSubscriptionResponse>;

public record ResumeSubscriptionResponse(string Message);
