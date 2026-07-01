using DreamSoft.Application.Features.Apps.LandingApp.TenantAuth.Dtos;
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
) : IRequest<TenantAuthResponse>;
