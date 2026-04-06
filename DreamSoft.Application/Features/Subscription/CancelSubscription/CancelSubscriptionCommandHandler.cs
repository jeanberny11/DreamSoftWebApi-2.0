using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Subscription.CancelSubscription;

public class CancelSubscriptionCommandHandler(
    ITenantRepository tenantRepository,
    ITenantSubscriptionRepository tenantSubscriptionRepository,
    ISubscriptionStatusRepository subscriptionStatusRepository,
    IPaymentGateway paymentGateway,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<CancelSubscriptionCommand, CancelSubscriptionResponse>
{
    public async Task<CancelSubscriptionResponse> Handle(
        CancelSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.TenantId
            ?? throw new UnauthorizedException("Unauthorized");

        // 1. Load tenant
        var tenant = await tenantRepository.GetByIdWithStatusAsync(tenantId, cancellationToken)
            ?? throw new NotFoundException("TenantNotFound", tenantId);

        // 2. Tenant must be ACTIVE
        if (tenant.Status.Code != TenantStatusCodes.Active)
            throw new ConflictException("InvalidTenantStatus");

        // 3. Find the subscription for the requested solution
        var subscription = await tenantSubscriptionRepository
            .GetByTenantAndSolutionAsync(tenantId, request.SolutionId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.TenantSubscriptionNotFound, tenantId, request.SolutionId);

        // 4. Only ACTIVE or TRIAL subscriptions can be cancelled
        var cancellableStatuses = new[]
        {
            SubscriptionStatusCodes.Active,
            SubscriptionStatusCodes.Trial,
            SubscriptionStatusCodes.PastDue
        };

        if (!cancellableStatuses.Contains(subscription.Status.Code))
            throw new ConflictException("SubscriptionNotCancellable");

        // 5. Must have a Stripe subscription ID
        if (string.IsNullOrWhiteSpace(subscription.StripeSubscriptionId))
            throw new ConflictException("NoStripeSubscriptionFound");

        // 6. Call the payment gateway to cancel on Stripe
        await paymentGateway.CancelSubscriptionAsync(
            subscription.StripeSubscriptionId,
            request.CancelImmediately,
            cancellationToken);

        // 7. If immediate — update local status to CANCELLED now.
        //    If at period end — Stripe will fire customer.subscription.deleted
        //    when the period expires and our webhook handler will update it then.
        //    We still mark it locally so the tenant UI can reflect the intent.
        if (request.CancelImmediately)
        {
            var cancelledStatus = await subscriptionStatusRepository
                .GetByCodeAsync(SubscriptionStatusCodes.Cancelled, cancellationToken)
                ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "SubscriptionStatus", "Code = CANCELLED");

            subscription.UpdateStatus(cancelledStatus.Id);
            subscription.Cancel(DateTime.UtcNow);
        }
        else
        {
            // Set EndDate to signal cancellation is pending — actual status
            // update happens via the Stripe webhook when the period ends
            subscription.Cancel(DateTime.UtcNow); // sets EndDate = now as a flag
        }

        await tenantSubscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var message = request.CancelImmediately
            ? "Subscription cancelled immediately."
            : "Subscription will be cancelled at the end of the current billing period.";

        return new CancelSubscriptionResponse(message);
    }
}
