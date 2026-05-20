using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Subscription.ChangePlan;

public class ChangePlanCommandHandler(
    ITenantRepository tenantRepository,
    ITenantSubscriptionRepository tenantSubscriptionRepository,
    IPlanPriceRepository planPriceRepository,
    ISubscriptionPlanRepository subscriptionPlanRepository,
    IPaymentGateway paymentGateway,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<ChangePlanCommand, ChangePlanResponse>
{
    public async Task<ChangePlanResponse> Handle(
        ChangePlanCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = currentTenantService.TenantId
            ?? throw new UnauthorizedException("Unauthorized");

        // 1. Load tenant
        var tenant = await tenantRepository.GetByIdWithStatusAsync(tenantId, cancellationToken)
            ?? throw new NotFoundException("TenantNotFound", tenantId);

        // 2. Tenant must be ACTIVE to change plans
        if (tenant.Status.Code != TenantStatusCodes.Active)
            throw new ConflictException("InvalidTenantStatus");

        // 3. Tenant must have a Stripe customer ID — means they've gone through checkout
        if (string.IsNullOrWhiteSpace(tenant.StripeCustomerId))
            throw new ConflictException("NoStripeCustomerFound");

        // 4. Load and validate the new PlanPrice
        var newPlanPrice = await planPriceRepository.GetByIdAsync(
            request.NewPlanPriceId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.PlanPriceNotFound, request.NewPlanPriceId);

        if (!newPlanPrice.IsActive)
            throw new ConflictException(ErrorMessageKeys.PlanPriceNotActive);

        if (string.IsNullOrWhiteSpace(newPlanPrice.StripePriceId))
            throw new ConflictException(ErrorMessageKeys.StripePriceIdNotConfigured);

        // 5. Load the new plan to get SolutionId
        var newPlan = await subscriptionPlanRepository.GetByIdAsync(
            newPlanPrice.PlanId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.SubscriptionPlanNotFound, newPlanPrice.PlanId);

        // 6. Find the tenant's current active subscription for this solution
        var currentSubscription = await tenantSubscriptionRepository
            .GetByTenantAndSolutionAsync(tenantId, newPlan.SolutionId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.TenantSubscriptionNotFound, tenantId, newPlan.SolutionId);

        // 7. Subscription must be ACTIVE or TRIAL to allow plan changes
        var changeableStatuses = new[]
        {
            SubscriptionStatusCodes.Active,
            SubscriptionStatusCodes.Trial
        };

        if (!changeableStatuses.Contains(currentSubscription.Status.Code))
            throw new ConflictException("SubscriptionNotChangeable");

        // 8. Prevent changing to the same plan price
        if (currentSubscription.PlanPriceId == request.NewPlanPriceId)
            throw new ConflictException("AlreadyOnThisPlan");

        // 9. Must have a Stripe subscription ID to update via gateway
        if (string.IsNullOrWhiteSpace(currentSubscription.StripeSubscriptionId))
            throw new ConflictException("NoStripeSubscriptionFound");

        // 10. Call the payment gateway to update the Stripe subscription
        await paymentGateway.ChangePlanAsync(
            new ChangePlanRequest(
                GatewaySubscriptionId: currentSubscription.StripeSubscriptionId,
                NewGatewayPriceId:     newPlanPrice.StripePriceId,
                ProrationImmediate:    request.ProrationImmediate),
            ct: cancellationToken);

        // 11. Update local TenantSubscription to reflect the new plan
        currentSubscription.UpdatePlan(newPlan.Id, newPlanPrice.Id);

        await tenantSubscriptionRepository.UpdateAsync(currentSubscription, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ChangePlanResponse("Plan updated successfully.");
    }
}
