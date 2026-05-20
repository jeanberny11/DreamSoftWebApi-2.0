using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Registration.RegisterTenant;

public record RegisterTenantCommand(
    string FirstName,
    string LastName,
    string CompanyName,
    string Email,
    string Password,
    string? TermsVersion,
    bool AcceptTerms
) : IRequest<RegisterTenantResponse>;

public record RegisterTenantResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    int TenantId,
    string Email,
     string FirstName,
    string LastName,
    string LogoUrl,
    string TenantStatus);
