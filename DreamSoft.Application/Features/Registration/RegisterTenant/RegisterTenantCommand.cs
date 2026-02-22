using MediatR;

namespace DreamSoft.Application.Features.Registration.RegisterTenant;

public record RegisterTenantCommand(
    string CompanyName,
    string Subdomain,
    string? TaxId,
    string Phone,
    string AddressLine1,
    int CountryId,
    int ProvinceId,
    int MunicipalityId,
    string AdminFirstName,
    string AdminLastName,
    string AdminEmail,
    string AdminPassword,
    int LanguageId,
    int CurrencyId
) : IRequest<RegisterTenantResponse>;

public record RegisterTenantResponse(string RegistrationToken);
