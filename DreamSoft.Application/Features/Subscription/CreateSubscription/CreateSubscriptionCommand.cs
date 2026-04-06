using MediatR;

namespace DreamSoft.Application.Features.Subscription.CreateSubscription;

public record CreateSubscriptionCommand(
    int TenantId,
    int PlanId,
    int PlanPriceId
) : IRequest<CreateSubscriptionResponse>;

public record CreateSubscriptionResponse(bool Success, int SubscriptionId, string CheckoutUrl);
