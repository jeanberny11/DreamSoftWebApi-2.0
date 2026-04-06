using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Subscription.RetryPayment;

public class RetryPaymentCommandHandler(
    ITenantRepository tenantRepository,
    ITenantSubscriptionRepository tenantSubscriptionRepository,
    IPlanPriceRepository planPriceRepository,
    ISubscriptionPlanRepository subscriptionPlanRepository,
    IPaymentGateway paymentGateway,
    IPaymentSettings paymentSettings,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RetryPaymentCommand, RetryPaymentResponse>
{
    public async Task<RetryPaymentResponse> Handle(
        RetryPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.TenantId
            ?? throw new UnauthorizedException("Unauthorized");

        // 1. Load subscription with status — validate it belongs to the authenticated tenant
        var subscription = await tenantSubscriptionRepository
            .GetByIdWithStatusAsync(request.SubscriptionId, cancellationToken)
            ?? throw new NotFoundException("SubscriptionNotFound", request.SubscriptionId);

        if (subscription.TenantId != tenantId)
            throw new ConflictException("SubscriptionNotOwnedByTenant");

        // 2. Only retryable when payment was never completed
        if (subscription.Status.Code != SubscriptionStatusCodes.ProcessingPayment &&
            subscription.Status.Code != SubscriptionStatusCodes.PaymentFailed)
        {
            throw new ConflictException("SubscriptionNotRetryable");
        }

        // 3. Validate StripePriceId is still configured on the plan price
        var planPrice = await planPriceRepository
            .GetByIdAsync(subscription.PlanPriceId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.PlanPriceNotFound, subscription.PlanPriceId);

        if (string.IsNullOrWhiteSpace(planPrice.StripePriceId))
            throw new ConflictException(ErrorMessageKeys.StripePriceIdNotConfigured);

        // 4. Load plan for trial days
        var plan = await subscriptionPlanRepository
            .GetByIdAsync(subscription.SubscriptionPlanId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.SubscriptionPlanNotFound, subscription.SubscriptionPlanId);

        // 5. Load tenant
        var tenant = await tenantRepository.GetByIdAsync(tenantId, cancellationToken)
            ?? throw new NotFoundException("TenantNotFound", tenantId);

        // 6. Get or create the Stripe customer
        var gatewayCustomerId = await paymentGateway.CreateOrGetCustomerAsync(
            tenantId:    tenantId,
            email:       tenant.Email,
            companyName: tenant.CompanyName,
            ct:          cancellationToken);

        if (string.IsNullOrWhiteSpace(tenant.StripeCustomerId))
        {
            tenant.SetStripeCustomerId(gatewayCustomerId);
            await tenantRepository.UpdateAsync(tenant, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // 7. Create a new Stripe checkout session
        var checkoutResult = await paymentGateway.CreateCheckoutSessionAsync(
            new CheckoutRequest(
                GatewayCustomerId: gatewayCustomerId,
                GatewayPriceId:    planPrice.StripePriceId,
                TenantId:          tenantId,
                TrialDays:         plan.TrialDays,
                SuccessUrl:        paymentSettings.SuccessUrl,
                CancelUrl:         paymentSettings.CancelUrl),
            ct: cancellationToken);

        // 8. Stamp the new StripeSessionId so the webhook finds this exact
        //    subscription when checkout.session.completed fires
        subscription.SetStripeSessionId(checkoutResult.GatewaySessionId);
        await tenantSubscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RetryPaymentResponse(checkoutResult.CheckoutUrl);
    }
}
