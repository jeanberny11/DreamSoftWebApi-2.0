using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.TenantProfile.EditTenantProfile;

public class EditTenantProfileCommandHandler(
    ITenantRepository tenantRepository,
    ICurrentTenantService currentTenantService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<EditTenantProfileCommand>
{
    public async Task Handle(
        EditTenantProfileCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = currentTenantService.TenantId
            ?? throw new UnauthorizedException("Unauthorized");

        var tenant = await tenantRepository.GetByIdAsync(tenantId, cancellationToken)
            ?? throw new NotFoundException("TenantNotFound", tenantId);

        tenant.UpdateProfile(
            request.FirstName,
            request.LastName,
            request.Phone,
            request.Website);

        tenant.UpdateCompanyName(request.CompanyName);

        tenant.UpdateAddress(
            request.AddressLine1,
            request.AddressLine2,
            request.PostalCode,
            request.CountryId,
            request.ProvinceId,
            request.MunicipalityId);

        tenant.UpdateLanguage(request.LanguageId);

        if (!tenant.OnboardingCompleted)
            tenant.CompleteOnboarding();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
