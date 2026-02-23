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
    int CurrencyId,
    /// <summary>
    /// Version identifier of the Terms of Service the user accepted during registration.
    /// When provided, the acceptance is recorded on the Tenant entity.
    /// Example: "2025-01-01"
    /// </summary>
    string? TermsVersion = null
) : IRequest<RegisterTenantResponse>;

public record RegisterTenantResponse(string RegistrationToken);
