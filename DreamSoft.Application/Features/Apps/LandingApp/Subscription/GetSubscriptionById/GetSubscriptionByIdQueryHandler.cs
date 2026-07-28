using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.LandingApp.Subscription.GetSubscriptions;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Subscription.GetSubscriptionById;

public class GetSubscriptionByIdQueryHandler(
    ITenantSubscriptionRepository tenantSubscriptionRepository,
    ITenantSubdomainRepository tenantSubdomainRepository,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<GetSubscriptionByIdQuery, TenantSubscriptionDto>
{
    public async Task<TenantSubscriptionDto> Handle(
        GetSubscriptionByIdQuery request, CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";

        var tenantId = currentTenantService.TenantId
            ?? throw new UnauthorizedException();

        var ts = await tenantSubscriptionRepository.GetByIdWithDetailsAsync(
            request.SubscriptionId, cancellationToken);

        // Ownership check — a foreign subscription is indistinguishable from a
        // nonexistent one, so both yield NotFound (no information leak).
        if (ts is null || ts.TenantId != tenantId)
            throw new NotFoundException("SubscriptionNotFound", request.SubscriptionId);

        var subdomain = await tenantSubdomainRepository.GetByTenantAndSolutionAsync(
            tenantId, ts.SolutionId, cancellationToken);

        return new TenantSubscriptionDto(
            Id: ts.Id,
            SolutionId: ts.SolutionId,
            SolutionCode: ts.Solution.Code,
            SolutionName: ts.Solution.GetTranslatedName(language),
            SolutionIcon: ts.Solution.Icon,
            Subdomain: subdomain?.Subdomain,
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
        );
    }
}
