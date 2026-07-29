using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.TenantProfile.GetTenantProfile;

public class GetTenantProfileQueryHandler(
    ITenantRepository tenantRepository,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<GetTenantProfileQuery, GetTenantProfileResponse>
{
    public async Task<GetTenantProfileResponse> Handle(
        GetTenantProfileQuery request,
        CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";
        var tenantId = currentTenantService.TenantId
            ?? throw new UnauthorizedException("Unauthorized");

        var tenant = await tenantRepository.GetByIdWithStatusAsync(tenantId, cancellationToken)
            ?? throw new NotFoundException("TenantNotFound", tenantId);

        return new GetTenantProfileResponse(
            Id: tenant.Id,
            FirstName: tenant.FirstName,
            LastName: tenant.LastName,
            CompanyName: tenant.CompanyName,
            Email: tenant.Email,
            Phone: tenant.Phone,
            Website: tenant.Website,
            AddressLine1: tenant.AddressLine1,
            AddressLine2: tenant.AddressLine2,
            Country: tenant.Country == null ? null : new TenantProfileCountryDto(
                CountryId: tenant.Country.Id,
                Code: tenant.Country.Code,
                Name: tenant.Country.Name
            ),
            Province: tenant.Province == null ? null : new TenantProfileProvinceDto(
                ProvinceId: tenant.Province.Id,
                Code: tenant.Province.Code,
                Name: tenant.Province.Name
            ),
            Municipality: tenant.Municipality == null ? null : new TenantProfileMunicipalityDto(
                MunicipalityId: tenant.Municipality.Id,
                Code: tenant.Municipality.Code,
                Name: tenant.Municipality.Name
            ),
            PostalCode: tenant.PostalCode,
            LanguageId: tenant.LanguageId,
            LogoUrl: tenant.LogoUrl,
            TenantStatus: tenant.Status.Translations.GetNameOrFallback(language, tenant.Status.Name),
            CreatedAt: tenant.CreatedAt,
            UpdatedAt: tenant.UpdatedAt
        );
    }
}