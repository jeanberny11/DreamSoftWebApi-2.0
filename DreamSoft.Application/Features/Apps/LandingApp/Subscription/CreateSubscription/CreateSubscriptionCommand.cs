using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Subscription.CreateSubscription;

public record CreateSubscriptionCommand(
    int PlanId,
    int PlanPriceId
) : IRequest<CreateSubscriptionResponse>;

public record CreateSubscriptionResponse(bool Success, int SubscriptionId, string CheckoutUrl);
