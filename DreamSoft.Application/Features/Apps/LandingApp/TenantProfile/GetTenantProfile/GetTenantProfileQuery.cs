namespace DreamSoft.Application.Features.Apps.LandingApp.TenantProfile.GetTenantProfile;
using MediatR;

public record GetTenantProfileQuery(string? Language = null) : IRequest<GetTenantProfileResponse>;

public record GetTenantProfileResponse(
    int Id,
    string FirstName,
    string LastName,
    string CompanyName,
    string Email,
    string Phone,
    string Website,
    string AddressLine1,
    string AddressLine2,
    TenantProfileCountryDto? Country,
    TenantProfileProvinceDto? Province,
    TenantProfileMunicipalityDto? Municipality,
    string PostalCode,
    int LanguageId,
    string LogoUrl,
    string TenantStatus,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record TenantProfileCountryDto(
    int CountryId,
    string Code,
    string Name
);

public record TenantProfileProvinceDto(
    int ProvinceId,
    string Code,
    string Name
);

public record TenantProfileMunicipalityDto(
    int MunicipalityId,
    string Code,
    string Name
);