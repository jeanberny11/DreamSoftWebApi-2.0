using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Subscription.GetChangePlanPreview;

public class GetChangePlanPreviewQueryHandler(
    ITenantRepository tenantRepository,
    ITenantSubscriptionRepository tenantSubscriptionRepository,
    IPlanPriceRepository planPriceRepository,
    ISubscriptionPlanRepository subscriptionPlanRepository,
    IPaymentGateway paymentGateway,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<GetChangePlanPreviewQuery, ChangePlanPreviewResponse>
{
    public async Task<ChangePlanPreviewResponse> Handle(
        GetChangePlanPreviewQuery request,
        CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";
        var tenantId = currentTenantService.TenantId
            ?? throw new UnauthorizedException("Unauthorized");

        // 1. Load tenant — must be ACTIVE (identical gate to ChangePlanCommand)
        var tenant = await tenantRepository.GetByIdWithStatusAsync(tenantId, cancellationToken)
            ?? throw new NotFoundException("TenantNotFound", tenantId);

        if (tenant.Status.Code != TenantStatusCodes.Active)
            throw new ConflictException("InvalidTenantStatus");

        // 2. Validate the new PlanPrice (cheap lookup — just to learn PlanId)
        var newPlanPriceLookup = await planPriceRepository.GetByIdAsync(
            request.NewPlanPriceId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.PlanPriceNotFound, request.NewPlanPriceId);

        if (!newPlanPriceLookup.IsActive)
            throw new ConflictException(ErrorMessageKeys.PlanPriceNotActive);

        if (string.IsNullOrWhiteSpace(newPlanPriceLookup.StripePriceId))
            throw new ConflictException(ErrorMessageKeys.StripePriceIdNotConfigured);

        // 3. Rich load of the new plan — Solution + translated PlanPrices,
        // everything the review page needs in one query
        var newPlan = await subscriptionPlanRepository.GetByIdWithSolutionAndPricesAsync(
            newPlanPriceLookup.PlanId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.SubscriptionPlanNotFound, newPlanPriceLookup.PlanId);

        var newPlanPrice = newPlan.PlanPrices.First(pp => pp.Id == request.NewPlanPriceId);

        // 4. Find the tenant's current subscription for this solution
        var subscription = await tenantSubscriptionRepository
            .GetByTenantAndSolutionAsync(tenantId, newPlan.SolutionId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.TenantSubscriptionNotFound, tenantId, newPlan.SolutionId);

        // 5. Subscription must be ACTIVE or TRIAL — identical gate to the real change
        var changeableStatuses = new[] { SubscriptionStatusCodes.Active, SubscriptionStatusCodes.Trial };
        if (!changeableStatuses.Contains(subscription.Status.Code))
            throw new ConflictException("SubscriptionNotChangeable");

        // 6. Prevent previewing a "change" to the same plan price
        if (subscription.PlanPriceId == request.NewPlanPriceId)
            throw new ConflictException("AlreadyOnThisPlan");

        // 7. Must have a Stripe subscription ID to preview against
        if (string.IsNullOrWhiteSpace(subscription.StripeSubscriptionId))
            throw new ConflictException("NoStripeSubscriptionFound");

        // 8. Ask the gateway to preview the invoice. No trial parameters are
        // sent — remaining trial days (if any) carry over untouched.
        var preview = await paymentGateway.PreviewPlanChangeAsync(
            subscription.StripeSubscriptionId,
            newPlanPrice.StripePriceId,
            request.ProrationImmediate,
            cancellationToken);

        // 9. Next billing date from the subscription's own current period end —
        // correct regardless of the proration choice, reusing the same
        // gateway method already used for period-end cancellation.
        var nextBillingDate = await paymentGateway.GetSubscriptionPeriodEndAsync(
            subscription.StripeSubscriptionId, cancellationToken);

        return new ChangePlanPreviewResponse(
            CurrentPlanName:         subscription.SubscriptionPlan.GetTranslatedName(language),
            CurrentPrice:            subscription.PlanPrice.Price,
            CurrentBillingCycleName: subscription.PlanPrice.BillingCycle.GetTranslatedName(language),

            SolutionName:  newPlan.Solution.GetTranslatedName(language),
            SolutionIcon:  newPlan.Solution.Icon,
            NewPlanName:   newPlan.GetTranslatedName(language),
            NewPrice:      newPlanPrice.Price,
            NewBillingCycleName: newPlanPrice.BillingCycle.GetTranslatedName(language),
            NewPlanPriceId: newPlanPrice.Id,

            CreditAmount:   preview.CreditAmount,
            ChargeAmount:   preview.ChargeAmount,
            AmountDueNow:   preview.AmountDueNow,
            Currency:       preview.Currency,
            NextBillingDate: nextBillingDate,
            NextBillingAmount: newPlanPrice.Price,
            ProrationDate:  preview.ProrationDate,
            ProrationImmediate: request.ProrationImmediate,

            HasActiveTrial: subscription.Status.Code == SubscriptionStatusCodes.Trial,
            TrialEndDate:   subscription.TrialEndDate
        );
    }
}
