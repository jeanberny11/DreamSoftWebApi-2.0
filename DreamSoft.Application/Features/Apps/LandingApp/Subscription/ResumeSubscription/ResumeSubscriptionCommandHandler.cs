using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Subscription.ResumeSubscription;

public class ResumeSubscriptionCommandHandler(
    ITenantSubscriptionRepository tenantSubscriptionRepository,
    IPaymentGateway paymentGateway,
    ICurrentTenantService currentTenantService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ResumeSubscriptionCommand, ResumeSubscriptionResponse>
{
    public async Task<ResumeSubscriptionResponse> Handle(
        ResumeSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = currentTenantService.TenantId
            ?? throw new UnauthorizedException("Unauthorized");

        // 1. Load subscription — must belong to the authenticated tenant
        var subscription = await tenantSubscriptionRepository
            .GetByIdWithStatusAsync(request.SubscriptionId, cancellationToken)
            ?? throw new NotFoundException("SubscriptionNotFound", request.SubscriptionId);

        if (subscription.TenantId != tenantId)
            throw new ConflictException("SubscriptionNotOwnedByTenant");

        // 2. Only resumable when a period-end cancellation is pending
        if (!subscription.HasPendingCancellation())
            throw new ConflictException("SubscriptionNotPendingCancellation");

        // 3. Must have a Stripe subscription to un-schedule
        if (string.IsNullOrWhiteSpace(subscription.StripeSubscriptionId))
            throw new ConflictException("NoStripeSubscriptionFound");

        // 4. Revert on Stripe first, then locally
        await paymentGateway.ResumeSubscriptionAsync(
            subscription.StripeSubscriptionId, cancellationToken);

        subscription.ClearScheduledCancellation();
        await tenantSubscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ResumeSubscriptionResponse("Subscription resumed successfully.");
    }
}
