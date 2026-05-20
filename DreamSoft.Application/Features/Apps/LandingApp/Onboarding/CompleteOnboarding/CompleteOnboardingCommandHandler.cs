using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Onboarding.CompleteOnboarding;

public class CompleteOnboardingCommandHandler(
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<CompleteOnboardingCommand, CompleteOnboardingResponse>
{
    public async Task<CompleteOnboardingResponse> Handle(
        CompleteOnboardingCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = currentTenantService.TenantId
            ?? throw new UnauthorizedException("Unauthorized");

        // 1. Load tenant with status
        var tenant = await tenantRepository.GetByIdWithStatusAsync(tenantId, cancellationToken)
            ?? throw new NotFoundException("TenantNotFound", tenantId);

        // 2. Must be PENDING_SUBSCRIPTION and not yet onboarded
        if (tenant.OnboardingCompleted)
            throw new ConflictException("OnboardingAlreadyComplete");

        // 4. Update profile — phone and website are optional
        tenant.UpdateProfile(
            firstName: tenant.FirstName,
            lastName:  tenant.LastName,
            phone:     request.Phone,
            website:   request.Website);

        // 5. Update address — all fields are optional
        tenant.UpdateAddress(
            addressLine1:   request.AddressLine1 ?? string.Empty,
            addressLine2:   request.AddressLine2,
            postalCode:     request.PostalCode,
            countryId:      request.CountryId,
            provinceId:     request.ProvinceId,
            municipalityId: request.MunicipalityId);

        // 6. Update preferred language
        if (request.LanguageId > 0)
            tenant.UpdateLanguage(request.LanguageId);

        // 8. Mark onboarding as complete — sets OnboardingCompleted = true
        tenant.CompleteOnboarding();

        // 9. Persist
        await tenantRepository.UpdateAsync(tenant, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 10. Redirect to subscription flow
        return new CompleteOnboardingResponse("/subscribe");
    }
}
