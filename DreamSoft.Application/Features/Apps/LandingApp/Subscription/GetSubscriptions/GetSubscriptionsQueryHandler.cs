using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Subscription.GetSubscriptions;

public class GetSubscriptionsQueryHandler(
    ITenantSubscriptionRepository tenantSubscriptionRepository,
    ITenantSubdomainRepository tenantSubdomainRepository,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<GetSubscriptionsQuery, IReadOnlyList<TenantSubscriptionDto>>
{
    public async Task<IReadOnlyList<TenantSubscriptionDto>> Handle(
        GetSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";

        var tenantId = currentTenantService.TenantId
            ?? throw new UnauthorizedException();

        var subscriptions = await tenantSubscriptionRepository.GetByTenantExcludingStatusAsync(
            tenantId, SubscriptionStatusCodes.Cancelled, cancellationToken);

        var subdomains = await tenantSubdomainRepository.GetByTenantIdAsync(
            tenantId, cancellationToken);

        return [.. subscriptions
            .Select(ts => new TenantSubscriptionDto(
                Id: ts.Id,
                SolutionId: ts.SolutionId,
                SolutionCode: ts.Solution.Code,
                SolutionName: ts.Solution.GetTranslatedName(language),
                SolutionIcon: ts.Solution.Icon,
                Subdomain: subdomains
                    .FirstOrDefault(sd => sd.SolutionId == ts.SolutionId)?.Subdomain,
                SubscriptionPlanId: ts.SubscriptionPlanId,
                SubscriptionPlanCode: ts.SubscriptionPlan.Code,
                SubscriptionPlanName: ts.SubscriptionPlan.GetTranslatedName(language),
                SubscriptionPlanDescription: ts.SubscriptionPlan.Translations.GetDescriptionOrFallback(language, ts.SubscriptionPlan.Description),
                TierLevel: ts.SubscriptionPlan.TierLevel,
                BillingCycleCode: ts.PlanPrice.BillingCycle.Code,
                BillingCycleName: ts.PlanPrice.BillingCycle.GetTranslatedName(language),
                BillingCycleDescription: ts.PlanPrice.BillingCycle.Translations.GetDescriptionOrFallback(language, ts.PlanPrice.BillingCycle.Description),
                Price: ts.PlanPrice.Price,
                StatusId: ts.StatusId,
                StatusCode: ts.Status.Code,
                StatusName: ts.Status.GetTranslatedName(language),
                StartDate: ts.StartDate,
                EndDate: ts.EndDate,
                TrialEndDate: ts.TrialEndDate,
                CancellationScheduledAt: ts.CancellationScheduledAt,
                Notes: ts.Notes
            ))];
    }
}
