using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.TenantAuth.LoginTenant;

/// <summary>
/// Authenticates a tenant account owner using their company email and password.
/// Issues a tenant-scoped access token and refresh token on success.
/// </summary>
public record LoginTenantCommand(
    string Email,
    string Password,
    bool RememberMe = false,
    string? DeviceInfo = null) : IRequest<LoginTenantResponse>;

public record LoginTenantResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    int TenantId,
    string Email,
    string FirstName,
    string LastName,
    string LogoUrl,
    bool OnboardingCompleted,
    bool EmailVerified,
    string StatusCode);
