using MediatR;

namespace DreamSoft.Application.Features.Auth.LoginTenant;

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
    string CompanyName,
    string Email,
    bool OnboardingCompleted);
