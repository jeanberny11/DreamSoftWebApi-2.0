using MediatR;

namespace DreamSoft.Application.Features.Registration.RegisterTenant;

public record RegisterTenantCommand(
    string FirstName,
    string LastName,
    string CompanyName,
    string Email,
    string Password,
    string? TermsVersion,
    bool AcceptTerms
) : IRequest<RegisterTenantResponse>;

public record RegisterTenantResponse(string Email, string Message);
