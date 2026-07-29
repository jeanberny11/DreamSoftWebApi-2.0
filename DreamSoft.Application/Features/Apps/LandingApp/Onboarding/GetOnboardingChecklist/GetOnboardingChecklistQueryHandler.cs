using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Onboarding.GetOnboardingChecklist;

public class GetOnboardingChecklistQueryHandler(
    ITenantRepository tenantRepository,
    ITenantSubscriptionRepository tenantSubscriptionRepository,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<GetOnboardingChecklistQuery, OnboardingChecklistResponse>
{
    public async Task<OnboardingChecklistResponse> Handle(
        GetOnboardingChecklistQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = currentTenantService.TenantId
            ?? throw new UnauthorizedException("Unauthorized");

        var tenant = await tenantRepository.GetByIdAsync(tenantId, cancellationToken)
            ?? throw new NotFoundException("TenantNotFound", tenantId);

        var activeSubscriptions = await tenantSubscriptionRepository
            .GetActiveByTenantAsync(tenantId, cancellationToken);

        return new OnboardingChecklistResponse(
            Verifications: new OnboardingVerificationsDto(
                EmailVerified: tenant.EmailVerified
            ),
            ProfileSetup: new OnboardingProfileSetupDto(
                Completed: tenant.OnboardingCompleted
            ),
            Subscription: new OnboardingSubscriptionDto(
                ActiveSubscriptions: activeSubscriptions.Count
            )
        );
    }
}
