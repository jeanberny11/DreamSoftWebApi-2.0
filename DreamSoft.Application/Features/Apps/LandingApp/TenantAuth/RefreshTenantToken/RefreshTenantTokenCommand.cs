using DreamSoft.Application.Features.Apps.LandingApp.TenantAuth.LoginTenant;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.TenantAuth.RefreshTenantToken;

public record RefreshTenantTokenCommand(string? RefreshToken) : IRequest<RefreshTenantTokenResponse>;

public record RefreshTenantTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    int TenantId,
    string Email,
    string FirstName,
    string LastName,
    string LogoUrl,
    string TenantStatus,
    bool EmailVerified,
    bool OnboardingCompleted);
