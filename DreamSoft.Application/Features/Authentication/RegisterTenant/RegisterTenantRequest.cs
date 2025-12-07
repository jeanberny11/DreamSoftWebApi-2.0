using DreamSoft.Application.Features.Authentication.RegisterTenant;
using MediatR;

namespace DreamSoft.Application.Features.Authentication.Requests;

/// <summary>
/// Request to register a new tenant and admin user
/// </summary>
public class RegisterTenantRequest : IRequest<RegisterTenantResponse>
{
    // Session token from email verification
    public string SessionToken { get; set; } = null!;

    // Tenant information
    public string CompanyName { get; set; } = null!;
    public string Subdomain { get; set; } = null!;
    public string? Phone { get; set; }
    public string? TaxId { get; set; }

    // Admin user information
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;

    // Preferences
    public int LanguageId { get; set; } = 1; // Default: Spanish

    // Terms acceptance
    public bool AcceptedTerms { get; set; }
}