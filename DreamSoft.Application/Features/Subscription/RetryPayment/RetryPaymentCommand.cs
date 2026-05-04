using MediatR;

namespace DreamSoft.Application.Features.Subscription.RetryPayment;

/// <summary>
/// Generates a new Stripe checkout session for a subscription that is in
/// PROCESSING_PAYMENT or PAYMENT_FAILED status, allowing the tenant to
/// retry their payment without going through the full registration flow again.
/// The SubscriptionId is provided by the frontend — the handler validates
/// it belongs to the authenticated tenant before proceeding.
/// </summary>
public record RetryPaymentCommand(
    int SubscriptionId
) : IRequest<RetryPaymentResponse>;

public record RetryPaymentResponse(string CheckoutUrl);
