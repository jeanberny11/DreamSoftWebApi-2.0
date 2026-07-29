namespace DreamSoft.Application.Features.Apps.LandingApp.TenantProfile.EditTenantProfile;
using MediatR;

public record EditTenantProfileCommand(
    string FirstName,
    string LastName,
    string CompanyName,
    string Phone,
    string? Website,
    string AddressLine1,
    string? AddressLine2,
    int CountryId,
    int ProvinceId,
    int MunicipalityId,
    string? PostalCode,
    int LanguageId
) : IRequest;