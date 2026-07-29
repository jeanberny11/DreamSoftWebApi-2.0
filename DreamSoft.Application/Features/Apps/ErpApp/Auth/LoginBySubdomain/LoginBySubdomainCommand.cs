using MediatR;

namespace DreamSoft.Application.Features.Apps.ErpApp.Auth.LoginBySubdomain;

/// <summary>
/// Authenticates a user by resolving the Tenant from the request subdomain
/// (populated by TenantResolutionMiddleware), then validating the username and password.
/// </summary>
public record LoginBySubdomainCommand(
    string Username,
    string Password,
    bool RememberMe = false,
    string? DeviceInfo = null) : IRequest<LoginResponse>;
